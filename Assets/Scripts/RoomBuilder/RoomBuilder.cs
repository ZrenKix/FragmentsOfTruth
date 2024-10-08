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
        public string name; // Name of the wall
        public bool isVisible = true;
        public List<WallOpening> openings = new List<WallOpening>();
        public Material customMaterial = null; // Custom material for this wall

        [System.NonSerialized]
        public bool foldout = false; // Foldout state for the wall in the inspector

        public WallData(string name)
        {
            this.name = name;
        }
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
            {
                string wallName = "Wall " + walls.Count;
                walls.Add(new WallData(wallName));
            }
        }
        else if (walls.Count > numWalls)
        {
            // Remove entries
            walls.RemoveRange(numWalls, walls.Count - numWalls);
        }

        // Update wall names if necessary
        for (int i = 0; i < walls.Count; i++)
        {
            if (string.IsNullOrEmpty(walls[i].name))
            {
                walls[i].name = "Wall " + i;
            }
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
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(vertexGO, "Create Vertex");
        #endif
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
                #if UNITY_EDITOR
                Undo.DestroyObjectImmediate(child.gameObject);
                #endif
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
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(vertexGO, "Insert Vertex");
        #endif
        vertexGO.transform.SetParent(transform, false);
        vertexGO.transform.localPosition = position;

        AdjustWallsList();
    }

    public void AddConnectedRoom(int wallIndex)
    {
        // Get the wall's vertices
        int nextIndex = (wallIndex + 1) % vertices.Count;
        VertexData vA = vertices[wallIndex];
        VertexData vB = vertices[nextIndex];

        // Calculate the direction along the wall and perpendicular to the wall
        Vector3 wallDir = (vB.localPosition - vA.localPosition).normalized;
        Vector3 perpDir = Vector3.Cross(wallDir, Vector3.up);

        // Parameters for the corridor and new room
        float corridorLength = 2f;
        float corridorWidth = 2f;
        float roomLength = 5f;
        float roomWidth = 5f;

        // Calculate the starting point of the corridor (midpoint of the wall)
        Vector3 wallMidPoint = (vA.localPosition + vB.localPosition) / 2f;

        // Corridor vertices (starting from the wall and extending outward)
        Vector3 c1 = wallMidPoint - perpDir * (corridorWidth / 2f);
        Vector3 c2 = wallMidPoint + perpDir * (corridorWidth / 2f);
        Vector3 c3 = c2 + wallDir * corridorLength;
        Vector3 c4 = c1 + wallDir * corridorLength;

        // Room vertices
        Vector3 r1 = c4 + wallDir * roomLength - perpDir * (roomWidth / 2f);
        Vector3 r2 = c3 + wallDir * roomLength + perpDir * (roomWidth / 2f);
        Vector3 r3 = r2 - wallDir * roomLength;
        Vector3 r4 = r1 - wallDir * roomLength;

        // List to hold new vertices
        List<VertexData> newVertices = new List<VertexData>();

        // Build the list of new vertices in the correct order
        // Starting from c1, moving around the corridor and new room
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 0), c1)); // After vB
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 1), c2));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 2), c3));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 3), r2));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 4), r3));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 5), r4));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 6), r1));
        newVertices.Add(new VertexData("Vertex_" + (vertices.Count + 7), c4));

        // Insert the new vertices into the vertices list at the correct position
        int insertIndex = nextIndex;
        vertices.InsertRange(insertIndex, newVertices);

        // Update walls
        AdjustWallsList();
        ReconstructVertexTransforms();
        UpdateRoom();
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

        // Note: You might need to replace this with your own triangulation method
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

    /// <summary>
    /// Check if a given GameObject is inside the room.
    /// </summary>
    /// <param name="gameObject">The GameObject to check.</param>
    /// <returns>True if the GameObject is inside the room, false otherwise.</returns>
    public bool IsObjectInsideRoom(GameObject gameObject)
    {
        // Get the object's world position
        Vector3 objectPosition = gameObject.transform.position;
        Vector2 objectPos2D = new Vector2(objectPosition.x, objectPosition.z);
        //Debug.Log($"Checking if object '{gameObject.name}' at world position {objectPosition} is inside the room.");

        // Get the room's floor vertices in world space (converted from local space)
        List<Vector2> floorVertices2D = new List<Vector2>();
        foreach (var vertexData in vertices)
        {
            // Convert the local vertex position to world position
            Vector3 worldPosition = transform.TransformPoint(vertexData.localPosition);
            Vector2 vertex2D = new Vector2(worldPosition.x, worldPosition.z);
            floorVertices2D.Add(vertex2D);
            //Debug.Log($"Room Vertex (World Space): {vertex2D}");
        }

        // Check if the object's 2D position is inside the polygon formed by the floor vertices
        bool isInPolygon = IsPointInPolygon(objectPos2D, floorVertices2D);
        //Debug.Log($"Is the object's 2D position {objectPos2D} inside the polygon: {isInPolygon}");

        if (!isInPolygon)
        {
            //Debug.Log("The object is outside the room in the XZ plane.");
            return false;
        }

        // Check if the object's Y position is within the room's height (floor and ceiling bounds in world space)
        float floorY = transform.position.y; // Get the world Y position of the room's floor (assuming floor is at room's Y position)
        float ceilingY = floorY + ceilingHeight; // The ceiling height is added to the floor's Y world position

        //Debug.Log($"Object Y position: {objectPosition.y}, Room height bounds (world space): [{floorY}, {ceilingY}]");

        if (objectPosition.y < floorY || objectPosition.y > ceilingY)
        {
            //Debug.Log("The object is outside the room in the Y axis.");
            return false;
        }

        //Debug.Log("The object is inside the room.");
        return true;
    }

    /// <summary>
    /// Determine if a point is inside a polygon using the ray-casting method.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="polygon">The polygon vertices in world space.</param>
    /// <returns>True if the point is inside the polygon, false otherwise.</returns>
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int numVertices = polygon.Count;
        bool isInside = false;

        //Debug.Log($"Checking if point {point} is inside polygon with {numVertices} vertices.");

        for (int i = 0, j = numVertices - 1; i < numVertices; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];

            // Log the edges being checked
            //Debug.Log($"Checking edge from {vi} to {vj}");

            // Check if the point is between the y-bounds of the edge
            bool intersect = ((vi.y > point.y) != (vj.y > point.y)) &&
                             (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x);

            if (intersect)
            {
                isInside = !isInside;
                //Debug.Log($"Ray intersects with edge. Flip isInside to: {isInside}");
            }
        }

        //Debug.Log($"Final result for point {point} inside polygon: {isInside}");
        return isInside;
    }

}
