using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RoomBuilder : MonoBehaviour
{
    public Material floorMaterial;
    public Material wallMaterial;
    public Material ceilingMaterial;
    public float ceilingHeight = 1f;

    [HideInInspector]
    public List<Transform> floorVertexTransforms = new List<Transform>();

    [HideInInspector]
    public List<bool> wallVisibility = new List<bool>();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    void OnEnable()
    {
        InitializeFloorVertices();
        UpdateRoom();
    }

    void OnValidate()
    {
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

    void InitializeFloorVertices()
    {
        // Find existing vertex GameObjects or create new ones if none exist
        floorVertexTransforms.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Vertex_"))
            {
                floorVertexTransforms.Add(child);
            }
        }

        if (floorVertexTransforms.Count == 0)
        {
            // Create default square room if no vertices exist
            CreateDefaultVertices();
        }

        AdjustWallVisibilityList();
    }

    void AdjustWallVisibilityList()
    {
        int numWalls = floorVertexTransforms.Count;
        if (wallVisibility.Count < numWalls)
        {
            // Add entries
            while (wallVisibility.Count < numWalls)
                wallVisibility.Add(true);
        }
        else if (wallVisibility.Count > numWalls)
        {
            // Remove entries
            wallVisibility.RemoveRange(numWalls, wallVisibility.Count - numWalls);
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
            CreateVertex(defaultPositions[i]);
        }
    }

    public void CreateVertex(Vector3 position)
    {
        GameObject vertexGO = new GameObject("Vertex_" + floorVertexTransforms.Count);
        vertexGO.transform.SetParent(transform, false);
        vertexGO.transform.localPosition = position;
        floorVertexTransforms.Add(vertexGO.transform);

        AdjustWallVisibilityList();
    }

    public void RemoveLastVertex()
    {
        if (floorVertexTransforms.Count > 0)
        {
            Transform lastVertex = floorVertexTransforms[floorVertexTransforms.Count - 1];
            floorVertexTransforms.RemoveAt(floorVertexTransforms.Count - 1);
            DestroyImmediate(lastVertex.gameObject);

            AdjustWallVisibilityList();
        }
    }

    public void UpdateRoom()
    {
        // Ensure we have at least 3 vertices to form a polygon
        if (floorVertexTransforms.Count < 3)
            return;

        AdjustWallVisibilityList();

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
        foreach (var vertexTransform in floorVertexTransforms)
        {
            floorVertices.Add(vertexTransform.localPosition);
        }

        // Create the room mesh
        Mesh roomMesh = new Mesh();
        roomMesh.name = "Room Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int>[] submeshTriangles = new List<int>[3]; // 0: Floor, 1: Ceiling, 2: Walls
        for (int i = 0; i < 3; i++)
            submeshTriangles[i] = new List<int>();

        // *** Floor ***
        int floorVertexOffset = vertices.Count;
        vertices.AddRange(floorVertices);

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
        int ceilingVertexOffset = vertices.Count;
        List<Vector3> ceilingVertices = new List<Vector3>();
        for (int i = 0; i < floorVertices.Count; i++)
        {
            Vector3 v = floorVertices[i];
            Vector3 ceilingVertex = new Vector3(v.x, v.y + ceilingHeight, v.z);
            ceilingVertices.Add(ceilingVertex);
            vertices.Add(ceilingVertex);

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
        int wallVertexOffset = vertices.Count;
        for (int i = 0; i < floorVertices.Count; i++)
        {
            if (!wallVisibility[i])
            {
                continue; // Skip this wall
            }

            int nextI = (i + 1) % floorVertices.Count;

            Vector3 v0 = floorVertices[i];
            Vector3 v1 = floorVertices[nextI];
            Vector3 v2 = ceilingVertices[nextI];
            Vector3 v3 = ceilingVertices[i];

            vertices.Add(v0); // index 0
            vertices.Add(v1); // index 1
            vertices.Add(v2); // index 2
            vertices.Add(v3); // index 3

            // Calculate wall length and height for UV mapping
            float wallLength = Vector3.Distance(v0, v1);
            float wallHeight = ceilingHeight;

            // Generate UVs for the wall
            uvs.Add(new Vector2(0, 0));                // v0
            uvs.Add(new Vector2(wallLength, 0));       // v1
            uvs.Add(new Vector2(wallLength, wallHeight)); // v2
            uvs.Add(new Vector2(0, wallHeight));          // v3

            int baseIndex = vertices.Count - 4;
            submeshTriangles[2].Add(baseIndex);
            submeshTriangles[2].Add(baseIndex + 1);
            submeshTriangles[2].Add(baseIndex + 2);

            submeshTriangles[2].Add(baseIndex + 2);
            submeshTriangles[2].Add(baseIndex + 3);
            submeshTriangles[2].Add(baseIndex);
        }

        // *** Assign vertices, UVs, and submeshes ***
        roomMesh.vertices = vertices.ToArray();
        roomMesh.uv = uvs.ToArray();
        roomMesh.subMeshCount = 3;

        roomMesh.SetTriangles(submeshTriangles[0], 0); // Floor
        roomMesh.SetTriangles(submeshTriangles[1], 1); // Ceiling
        roomMesh.SetTriangles(submeshTriangles[2], 2); // Walls

        roomMesh.RecalculateNormals();
        roomMesh.RecalculateBounds();

        // Assign mesh to MeshFilter
        meshFilter.sharedMesh = roomMesh;

        // Assign mesh to MeshCollider
        meshCollider.sharedMesh = null; // Clear the old mesh to force an update
        meshCollider.sharedMesh = roomMesh;

        // *** Assign materials ***
        meshRenderer.sharedMaterials = new Material[] { floorMaterial, ceilingMaterial, wallMaterial };
    }
}
