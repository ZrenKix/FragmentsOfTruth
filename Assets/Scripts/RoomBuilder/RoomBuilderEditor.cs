using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomBuilder))]
public class RoomBuilderEditor : Editor
{
    private RoomBuilder roomBuilder;
    private bool showVerticesList = true;
    private SerializedProperty wallVisibilityProp;

    private void OnEnable()
    {
        roomBuilder = (RoomBuilder)target;
        wallVisibilityProp = serializedObject.FindProperty("wallVisibility");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("floorMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingHeight"));

        // Display and edit vertex positions
        showVerticesList = EditorGUILayout.Foldout(showVerticesList, "Floor Vertices");
        if (showVerticesList)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < roomBuilder.floorVertexTransforms.Count; i++)
            {
                Transform vertexTransform = roomBuilder.floorVertexTransforms[i];

                EditorGUILayout.BeginHorizontal();

                // Display the index
                EditorGUILayout.LabelField("Vertex " + i, GUILayout.Width(70));

                // Display and edit the localPosition of the vertex
                Vector3 oldPosition = vertexTransform.localPosition;
                Vector3 newPosition = EditorGUILayout.Vector3Field("", oldPosition);

                if (newPosition != oldPosition)
                {
                    Undo.RecordObject(vertexTransform, "Move Vertex");
                    vertexTransform.localPosition = newPosition;
                    roomBuilder.UpdateRoom();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Vertex"))
            {
                Undo.RecordObject(roomBuilder, "Add Vertex");
                Vector3 newPosition = Vector3.zero;
                if (roomBuilder.floorVertexTransforms.Count > 0)
                {
                    // Place new vertex near the last one
                    newPosition = roomBuilder.floorVertexTransforms[roomBuilder.floorVertexTransforms.Count - 1].localPosition + Vector3.right * 1f;
                }
                roomBuilder.CreateVertex(newPosition);
                roomBuilder.UpdateRoom();
            }
            if (GUILayout.Button("Remove Last Vertex"))
            {
                Undo.RecordObject(roomBuilder, "Remove Vertex");
                roomBuilder.RemoveLastVertex();
                roomBuilder.UpdateRoom();
            }
            EditorGUILayout.EndHorizontal();
        }

        // Display wall visibility toggles
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Wall Visibility", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        for (int i = 0; i < wallVisibilityProp.arraySize; i++)
        {
            SerializedProperty wallVisibleProp = wallVisibilityProp.GetArrayElementAtIndex(i);
            int nextIndex = (i + 1) % wallVisibilityProp.arraySize;
            string wallLabel = $"Wall {i} (Vertex {i} - Vertex {nextIndex})";
            wallVisibleProp.boolValue = EditorGUILayout.Toggle(wallLabel, wallVisibleProp.boolValue);
        }

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            roomBuilder.UpdateRoom();
        }
    }

    private void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < roomBuilder.floorVertexTransforms.Count; i++)
        {
            Transform vertexTransform = roomBuilder.floorVertexTransforms[i];
            Vector3 worldPos = vertexTransform.position;

            // Display a position handle for each vertex
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vertexTransform, "Move Vertex");
                vertexTransform.position = newWorldPos;
                roomBuilder.UpdateRoom();
            }
        }
    }
}
