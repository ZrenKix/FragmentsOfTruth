using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
public class RoomBuilder : MonoBehaviour
{
    public Material floorMaterial;
    public Material wallMaterial; // Default wall material
    public Material ceilingMaterial;
    public float ceilingHeight = 1f;

    [System.Serializable]
    public class VertexData
    {
        public string name;
        public Vector3 localPosition;

        public VertexData(string name, Vector3 localPosition)
        {
            this.name = name;
            this.localPosition = localPosition;
        }
    }

    [SerializeField]
    public List<VertexData> vertices = new List<VertexData>();

    [System.Serializable]
    public class WallOpening
    {
        public float position = 1f; // Position along the wall (from 0 to wall length)
        public float width = 1f;    // Width of the opening
        public float height = 1f;   // Height of the opening
        public float bottom = 0f;   // Distance from the floor to the bottom of the opening
    }

    [System.Serializable]
    public class WallData
    {
        public bool isVisible = true;
        public List<WallOpening> openings = new List<WallOpening>();
        public Material customMaterial = null; // Custom material for this wall

        [System.NonSerialized]
        public bool foldout = false; // Foldout state for the wall in the inspector
    }

    [SerializeField]
    public List<WallData> walls = new List<WallData>();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void OnEnable()
    {
        // Create default vertices if none exist
        if (vertices.Count == 0)
        {
            CreateDefaultVertices();
        }

        AdjustWallsList();
        ReconstructVertexTransforms();
        UpdateRoom();
    }

    void OnValidate()
    {
        AdjustWallsList();
        UpdateRoom();
    }

    void Update()
    {
        // Check for changes in the editor and update the room accordingly
        if (!Application.isPlaying)
        {
            UpdateRoom();
        }
    }

    public void ReconstructVertexTransforms()
    {
        // Ensure each vertex has a corresponding child GameObject
        foreach (var vertexData in vertices)
        {
            Transform child = transform.Find(vertexData.name);
            if (child == null)
            {
                // Create a new GameObject for the vertex
                GameObject vertexGO = new GameObject(vertexData.name);
                vertexGO.transform.SetParent(transform, false);
                vertexGO.transform.localPosition = vertexData.localPosition;
            }
            else
            {
                // Update the position
                child.localPosition = vertexData.localPosition;
            }
        }

        // Remove any extra child GameObjects that are not in the vertices list
        List<string> vertexNames = new List<string>();
        foreach (var v in vertices)
        {
            vertexNames.Add(v.name);
        }

        List<Transform> childrenToRemove = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (!vertexNames.Contains(child.name))
            {
                childrenToRemove.Add(child);
            }
        }

        foreach (var child in childrenToRemove)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    void AdjustWallsList()
    {
        int numWalls = vertices.Count;
        if (walls.Count < numWalls)
        {
            // Add entries
            while (walls.Count < numWalls)
                walls.Add(new WallData());
        }
        else if (walls.Count > numWalls)
        {
            // Remove entries
            walls.RemoveRange(numWalls, walls.Count - numWalls);
        }
    }

    void CreateDefaultVertices()
    {
        // Create 4 default vertices for a square room
        Vector3[] defaultPositions = new Vector3[]
        {
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(5, 0, 5),
            new Vector3(-5, 0, 5)
        };

        for (int i = 0; i < defaultPositions.Length; i++)
        {
            CreateVertex(defaultPositions[i], "Vertex_" + i);
        }
    }

    public void CreateVertex(Vector3 position, string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "Vertex_" + vertices.Count;
        }

        VertexData vertexData = new VertexData(name, position);
        vertices.Add(vertexData);

        // Create a corresponding GameObject
        GameObject vertexGO = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(vertexGO, "Create Vertex");
        vertexGO.transform.SetParent(transform, false);
        vertexGO.transform.localPosition = position;

        AdjustWallsList();
    }

    public void RemoveVertex(int index)
    {
        if (index >= 0 && index < vertices.Count)
        {
            // Remove the vertex data
            VertexData vertexData = vertices[index];
            vertices.RemoveAt(index);

            // Destroy the corresponding GameObject
            Transform child = transform.Find(vertexData.name);
            if (child != null)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            AdjustWallsList();
        }
    }

    public void InsertVertex(int index, Vector3 position, string name)
    {
        VertexData vertexData = new VertexData(name, position);
        vertices.Insert(index, vertexData);

        // Create a corresponding GameObject
        GameObject vertexGO = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(vertexGO, "Insert Vertex");
        vertexGO.transform.SetParent(transform, false);
        vertexGO.transform.localPosition = position;

        AdjustWallsList();
    }

    public void UpdateRoom()
    {
        // Ensure we have at least 3 vertices to form a polygon
        if (vertices.Count < 3)
            return;

        AdjustWallsList();

        // Get or add MeshFilter, MeshRenderer, and MeshCollider on this GameObject
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        // Get floor vertices positions
        List<Vector3> floorVertices = new List<Vector3>();
        foreach (var vertexData in vertices)
        {
            floorVertices.Add(vertexData.localPosition);
        }

        // Create the room mesh
        Mesh roomMesh = new Mesh();
        roomMesh.name = "Room Mesh";

        List<Vector3> verticesList = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int>[] submeshTriangles = new List<int>[2]; // 0: Floor, 1: Ceiling
        for (int i = 0; i < 2; i++)
            submeshTriangles[i] = new List<int>();

        // *** Floor ***
        int floorVertexOffset = verticesList.Count;
        verticesList.AddRange(floorVertices);

        // Generate UVs for the floor
        for (int i = 0; i < floorVertices.Count; i++)
        {
            Vector3 v = floorVertices[i];
            Vector2 uv = new Vector2(v.x, v.z); // Use x and z as UV coordinates
            uvs.Add(uv);
        }

        Triangulator floorTriangulator = new Triangulator(floorVertices);
        int[] floorIndices = floorTriangulator.Triangulate();
        submeshTriangles[0].AddRange(floorIndices);

        // *** Ceiling ***
        int ceilingVertexOffset = verticesList.Count;
        List<Vector3> ceilingVertices = new List<Vector3>();
        for (int i = 0; i < floorVertices.Count; i++)
        {
            Vector3 v = floorVertices[i];
            Vector3 ceilingVertex = new Vector3(v.x, v.y + ceilingHeight, v.z);
            ceilingVertices.Add(ceilingVertex);
            verticesList.Add(ceilingVertex);

            // Generate UVs for the ceiling
            Vector2 uv = new Vector2(v.x, v.z); // Use x and z as UV coordinates
            uvs.Add(uv);
        }

        Triangulator ceilingTriangulator = new Triangulator(ceilingVertices);
        int[] ceilingIndices = ceilingTriangulator.Triangulate();
        System.Array.Reverse(ceilingIndices); // Reverse indices to flip normals

        for (int i = 0; i < ceilingIndices.Length; i++)
            ceilingIndices[i] += ceilingVertexOffset;
        submeshTriangles[1].AddRange(ceilingIndices);

        // *** Walls ***
        // Collect wall triangles per material
        Dictionary<Material, List<int>> wallSubmeshTriangles = new Dictionary<Material, List<int>>();

        // Ensure the default wall material is included
        Material defaultWallMaterial = wallMaterial != null ? wallMaterial : new Material(Shader.Find("Standard"));

        if (!wallSubmeshTriangles.ContainsKey(defaultWallMaterial))
        {
            wallSubmeshTriangles[defaultWallMaterial] = new List<int>();
        }

        for (int i = 0; i < floorVertices.Count; i++)
        {
            WallData wallData = walls[i];

            if (!wallData.isVisible)
            {
                continue; // Skip this wall
            }

            int nextI = (i + 1) % floorVertices.Count;

            Vector3 v0 = floorVertices[i];
            Vector3 v1 = floorVertices[nextI];
            Vector3 v2 = ceilingVertices[nextI];
            Vector3 v3 = ceilingVertices[i];

            // Calculate wall direction and length
            Vector3 wallDir = (v1 - v0).normalized;
            float wallLength = Vector3.Distance(v0, v1);
            float wallHeight = ceilingHeight;

            // Handle wall openings
            List<WallOpening> openings = wallData.openings;

            // Determine the material for this wall
            Material wallMat = wallData.customMaterial != null ? wallData.customMaterial : defaultWallMaterial;

            // Ensure the material is in the dictionary
            if (!wallSubmeshTriangles.ContainsKey(wallMat))
            {
                wallSubmeshTriangles[wallMat] = new List<int>();
            }

            // Generate wall mesh with openings
            GenerateWallWithOpenings(
                v0, v1, v2, v3,
                wallDir, wallLength, wallHeight,
                openings,
                verticesList, uvs, wallSubmeshTriangles[wallMat]
            );
        }

        // *** Assign vertices, UVs, and submeshes ***
        int totalSubmeshes = 2 + wallSubmeshTriangles.Count;
        roomMesh.subMeshCount = totalSubmeshes;

        roomMesh.vertices = verticesList.ToArray();
        roomMesh.uv = uvs.ToArray();

        // Set floor and ceiling triangles
        roomMesh.SetTriangles(submeshTriangles[0], 0); // Floor
        roomMesh.SetTriangles(submeshTriangles[1], 1); // Ceiling

        // Prepare materials list
        List<Material> meshMaterials = new List<Material>();
        meshMaterials.Add(floorMaterial);   // Submesh 0
        meshMaterials.Add(ceilingMaterial); // Submesh 1

        // Set wall triangles and materials
        int submeshIndex = 2;
        foreach (var kvp in wallSubmeshTriangles)
        {
            Material mat = kvp.Key;
            List<int> triangles = kvp.Value;
            roomMesh.SetTriangles(triangles, submeshIndex);
            meshMaterials.Add(mat);
            submeshIndex++;
        }

        roomMesh.RecalculateNormals();
        roomMesh.RecalculateBounds();

        // Assign mesh to MeshFilter
        meshFilter.sharedMesh = roomMesh;

        // Assign mesh to MeshCollider
        meshCollider.sharedMesh = null; // Clear the old mesh to force an update
        meshCollider.sharedMesh = roomMesh;

        // *** Assign materials ***
        meshRenderer.sharedMaterials = meshMaterials.ToArray();
    }

    void GenerateWallWithOpenings(
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        Vector3 wallDir, float wallLength, float wallHeight,
        List<WallOpening> openings,
        List<Vector3> verticesList, List<Vector2> uvs, List<int> triangles)
    {
        // Define the wall segments between openings
        float currentPos = 0f;
        float currentU = 0f; // Cumulative U coordinate for UV mapping

        // Sort openings by position
        openings.Sort((a, b) => a.position.CompareTo(b.position));

        foreach (var opening in openings)
        {
            // Opening constraints
            float openingStart = Mathf.Max(0, opening.position);
            float openingEnd = Mathf.Min(wallLength, opening.position + opening.width);

            // Add wall segment before opening
            if (openingStart > currentPos)
            {
                AddWallSegment(
                    v0, wallDir, currentPos, openingStart,
                    wallHeight, currentU, verticesList, uvs, triangles
                );
                // Update currentU
                currentU += openingStart - currentPos;
            }

            // Add opening edges (sides, top, bottom)
            AddOpeningVertices(
                v0, wallDir, openingStart, openingEnd, opening.bottom, opening.height,
                wallHeight, currentU, verticesList, uvs, triangles
            );

            // Update currentPos and currentU
            currentPos = openingEnd;
            currentU += openingEnd - openingStart;
        }

        // Add remaining wall segment after last opening
        if (currentPos < wallLength)
        {
            AddWallSegment(
                v0, wallDir, currentPos, wallLength,
                wallHeight, currentU, verticesList, uvs, triangles
            );
            // Update currentU
            currentU += wallLength - currentPos;
        }
    }

    void AddWallSegment(
        Vector3 wallStart, Vector3 wallDir,
        float start, float end, float wallHeight, float startU,
        List<Vector3> verticesList, List<Vector2> uvs, List<int> triangles)
    {
        Vector3 startBottom = wallStart + wallDir * start;
        Vector3 endBottom = wallStart + wallDir * end;
        Vector3 startTop = startBottom + Vector3.up * wallHeight;
        Vector3 endTop = endBottom + Vector3.up * wallHeight;

        int baseIndex = verticesList.Count;

        verticesList.Add(startBottom); // index 0
        verticesList.Add(endBottom);   // index 1
        verticesList.Add(endTop);      // index 2
        verticesList.Add(startTop);    // index 3

        // UVs
        float segmentLength = end - start;
        float endU = startU + segmentLength;

        uvs.Add(new Vector2(startU, 0));         // v0
        uvs.Add(new Vector2(endU, 0));           // v1
        uvs.Add(new Vector2(endU, wallHeight));  // v2
        uvs.Add(new Vector2(startU, wallHeight)); // v3

        // Triangles
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);

        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);
        triangles.Add(baseIndex);
    }

    void AddOpeningVertices(
        Vector3 wallStart, Vector3 wallDir,
        float openingStart, float openingEnd,
        float openingBottom, float openingHeight, float wallHeight,
        float startU,
        List<Vector3> verticesList, List<Vector2> uvs, List<int> triangles)
    {
        // Left side of opening
        Vector3 leftBottom = wallStart + wallDir * openingStart;
        Vector3 leftTop = leftBottom + Vector3.up * wallHeight;

        Vector3 leftOpeningBottom = leftBottom + Vector3.up * openingBottom;
        Vector3 leftOpeningTop = leftOpeningBottom + Vector3.up * openingHeight;

        // Right side of opening
        Vector3 rightBottom = wallStart + wallDir * openingEnd;
        Vector3 rightTop = rightBottom + Vector3.up * wallHeight;

        Vector3 rightOpeningBottom = rightBottom + Vector3.up * openingBottom;
        Vector3 rightOpeningTop = rightOpeningBottom + Vector3.up * openingHeight;

        float openingLength = openingEnd - openingStart;
        float endU = startU + openingLength;

        // Add side walls (edges of the opening)
        // Left side
        AddQuad(
            leftBottom, leftOpeningBottom, leftOpeningTop, leftTop,
            startU, startU, wallHeight, openingBottom, openingHeight,
            verticesList, uvs, triangles
        );

        // Right side
        AddQuad(
            rightBottom, rightOpeningBottom, rightOpeningTop, rightTop,
            endU, endU, wallHeight, openingBottom, openingHeight,
            verticesList, uvs, triangles
        );

        // Add top of opening
        AddQuad(
            leftOpeningTop, rightOpeningTop, rightTop, leftTop,
            startU, endU, wallHeight, openingBottom + openingHeight, wallHeight - (openingBottom + openingHeight),
            verticesList, uvs, triangles
        );

        // Add bottom of opening
        AddQuad(
            leftBottom, rightBottom, rightOpeningBottom, leftOpeningBottom,
            startU, endU, wallHeight, 0, openingBottom,
            verticesList, uvs, triangles
        );
    }

    void AddQuad(
        Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3,
        float u0, float u1, float wallHeight, float vStart, float vHeight,
        List<Vector3> verticesList, List<Vector2> uvs, List<int> triangles)
    {
        int baseIndex = verticesList.Count;

        verticesList.Add(v0);
        verticesList.Add(v1);
        verticesList.Add(v2);
        verticesList.Add(v3);

        // UVs
        uvs.Add(new Vector2(u0, vStart));
        uvs.Add(new Vector2(u1, vStart));
        uvs.Add(new Vector2(u1, vStart + vHeight));
        uvs.Add(new Vector2(u0, vStart + vHeight));

        // Triangles
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);

        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);
        triangles.Add(baseIndex);
    }
}
