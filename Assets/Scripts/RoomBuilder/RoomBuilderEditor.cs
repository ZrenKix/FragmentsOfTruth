using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RoomBuilder))]
public class RoomBuilderEditor : Editor
{
    private RoomBuilder roomBuilder;
    private bool showVerticesList = true;
    private SerializedProperty wallsProp;

    private void OnEnable()
    {
        roomBuilder = (RoomBuilder)target;
        wallsProp = serializedObject.FindProperty("walls");

        // Subscribe to Undo/Redo events
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe from Undo/Redo events
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Ensure roomBuilder and wallsProp are not null
        if (roomBuilder == null)
        {
            roomBuilder = (RoomBuilder)target;
        }

        if (wallsProp == null)
        {
            wallsProp = serializedObject.FindProperty("walls");
        }

        // *** Materials Section ***
        EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("floorMaterial"), new GUIContent("Floor Material"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallMaterial"), new GUIContent("Default Wall Material"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingMaterial"), new GUIContent("Ceiling Material"));
        EditorGUILayout.Space();

        // *** Ceiling Section ***
        EditorGUILayout.LabelField("Ceiling", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingHeight"), new GUIContent("Ceiling Height"));
        EditorGUILayout.Space();

        // *** Vertices Section ***
        showVerticesList = EditorGUILayout.Foldout(showVerticesList, "Vertices");
        if (showVerticesList)
        {
            EditorGUILayout.LabelField("Floor Vertices", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < roomBuilder.vertices.Count; i++)
            {
                var vertexData = roomBuilder.vertices[i];

                EditorGUILayout.BeginHorizontal();

                // Display and edit the name of the vertex
                string oldName = vertexData.name;
                string newName = EditorGUILayout.TextField(oldName, GUILayout.Width(100));

                if (newName != oldName)
                {
                    Undo.RecordObject(roomBuilder, "Rename Vertex");
                    vertexData.name = newName;

                    // Update the name of the corresponding GameObject
                    Transform vertexTransform = roomBuilder.transform.Find(oldName);
                    if (vertexTransform != null)
                    {
                        Undo.RecordObject(vertexTransform.gameObject, "Rename Vertex GameObject");
                        vertexTransform.name = newName;
                    }
                }

                // Display and edit the localPosition of the vertex
                Vector3 oldPosition = vertexData.localPosition;
                Vector3 newPosition = EditorGUILayout.Vector3Field("", oldPosition);

                if (newPosition != oldPosition)
                {
                    Undo.RecordObject(roomBuilder, "Move Vertex");
                    vertexData.localPosition = newPosition;

                    // Update the position of the corresponding GameObject
                    Transform vertexTransform = roomBuilder.transform.Find(vertexData.name);
                    if (vertexTransform != null)
                    {
                        vertexTransform.localPosition = newPosition;
                    }

                    roomBuilder.UpdateRoom();
                    serializedObject.Update(); // Update serialized properties
                }

                // Insert Vertex After
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    Vector3 positionA = vertexData.localPosition;
                    Vector3 positionB = roomBuilder.vertices[(i + 1) % roomBuilder.vertices.Count].localPosition;
                    Vector3 newVertexPosition = (positionA + positionB) / 2f;
                    string newVertexName = "Vertex_" + roomBuilder.vertices.Count;

                    Undo.RegisterCompleteObjectUndo(roomBuilder, "Insert Vertex");
                    roomBuilder.InsertVertex(i + 1, newVertexPosition, newVertexName);
                    roomBuilder.UpdateRoom();
                    serializedObject.Update(); // Update serialized properties
                    break; // Break to avoid modifying collection during iteration
                }

                // Remove Vertex
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    if (roomBuilder.vertices.Count > 3)
                    {
                        Undo.RegisterCompleteObjectUndo(roomBuilder, "Remove Vertex");
                        roomBuilder.RemoveVertex(i);
                        roomBuilder.UpdateRoom();
                        serializedObject.Update(); // Update serialized properties
                        break; // Break to avoid modifying collection during iteration
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Cannot Remove Vertex", "A room must have at least three vertices.", "OK");
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        // *** Walls Section ***
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Walls", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        int wallsCount = Mathf.Min(wallsProp.arraySize, roomBuilder.vertices.Count);

        for (int i = 0; i < wallsCount; i++)
        {
            SerializedProperty wallProp = wallsProp.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = wallProp.FindPropertyRelative("name");
            SerializedProperty isVisibleProp = wallProp.FindPropertyRelative("isVisible");
            SerializedProperty openingsProp = wallProp.FindPropertyRelative("openings");
            SerializedProperty customMaterialProp = wallProp.FindPropertyRelative("customMaterial");

            int nextIndex = (i + 1) % roomBuilder.vertices.Count;
            string vertexNameA = roomBuilder.vertices[i].name;
            string vertexNameB = roomBuilder.vertices[nextIndex].name;

            string wallLabel = $"{nameProp.stringValue} ({vertexNameA} - {vertexNameB})";

            // Use the foldout state from WallData
            var wallData = roomBuilder.walls[i];
            wallData.foldout = EditorGUILayout.Foldout(wallData.foldout, wallLabel);
            if (wallData.foldout)
            {
                EditorGUI.indentLevel++;

                // Wall name
                EditorGUI.BeginChangeCheck();
                string newWallName = EditorGUILayout.TextField("Wall Name", nameProp.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(roomBuilder, "Rename Wall");
                    nameProp.stringValue = newWallName;
                    // Update the wall label
                    wallLabel = $"{newWallName} ({vertexNameA} - {vertexNameB})";
                }

                // Wall visibility
                EditorGUI.BeginChangeCheck();
                bool newIsVisible = EditorGUILayout.Toggle("Visible", isVisibleProp.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(roomBuilder, "Toggle Wall Visibility");
                    isVisibleProp.boolValue = newIsVisible;
                    roomBuilder.UpdateRoom();
                }

                // Custom Material
                EditorGUI.BeginChangeCheck();
                Material newMaterial = (Material)EditorGUILayout.ObjectField("Custom Material", customMaterialProp.objectReferenceValue, typeof(Material), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(roomBuilder, "Change Wall Material");
                    customMaterialProp.objectReferenceValue = newMaterial;
                    roomBuilder.UpdateRoom();
                }

                // Add Connected Room Button
                if (GUILayout.Button("Add Connected Room"))
                {
                    Undo.RegisterCompleteObjectUndo(roomBuilder, "Add Connected Room");
                    roomBuilder.AddConnectedRoom(i);
                    serializedObject.Update(); // Update serialized properties
                    break; // Break to avoid modifying collection during iteration
                }

                // Openings
                SerializedProperty openingsArray = openingsProp;
                int openingsCount = openingsArray.arraySize;

                EditorGUILayout.LabelField("Openings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                for (int j = 0; j < openingsCount; j++)
                {
                    SerializedProperty openingProp = openingsArray.GetArrayElementAtIndex(j);
                    SerializedProperty positionProp = openingProp.FindPropertyRelative("position");
                    SerializedProperty widthProp = openingProp.FindPropertyRelative("width");
                    SerializedProperty heightProp = openingProp.FindPropertyRelative("height");
                    SerializedProperty bottomProp = openingProp.FindPropertyRelative("bottom");

                    EditorGUILayout.LabelField($"Opening {j + 1}", EditorStyles.miniBoldLabel);
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();
                    float newPosition = EditorGUILayout.FloatField("Position", positionProp.floatValue);
                    float newWidth = EditorGUILayout.FloatField("Width", widthProp.floatValue);
                    float newHeight = EditorGUILayout.FloatField("Height", heightProp.floatValue);
                    float newBottom = EditorGUILayout.FloatField("Bottom", bottomProp.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(roomBuilder, "Modify Opening");
                        positionProp.floatValue = newPosition;
                        widthProp.floatValue = newWidth;
                        heightProp.floatValue = newHeight;
                        bottomProp.floatValue = newBottom;
                        roomBuilder.UpdateRoom();
                    }

                    // Remove opening button
                    if (GUILayout.Button("Remove Opening"))
                    {
                        Undo.RecordObject(roomBuilder, "Remove Opening");
                        openingsArray.DeleteArrayElementAtIndex(j);
                        roomBuilder.UpdateRoom();
                        break;
                    }

                    EditorGUI.indentLevel--;
                }

                // Add opening button
                if (GUILayout.Button("Add Opening"))
                {
                    Undo.RecordObject(roomBuilder, "Add Opening");
                    openingsArray.arraySize++;
                    SerializedProperty newOpeningProp = openingsArray.GetArrayElementAtIndex(openingsCount);
                    newOpeningProp.FindPropertyRelative("position").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("width").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("height").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("bottom").floatValue = 0f;
                    roomBuilder.UpdateRoom();
                }

                EditorGUI.indentLevel -= 2;
            }
        }

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            roomBuilder.UpdateRoom();
            serializedObject.Update(); // Update serialized properties
        }
    }

    private void OnSceneGUI()
    {
        // Highlight the selected vertex
        Transform selectedTransform = Selection.activeTransform;

        for (int i = 0; i < roomBuilder.vertices.Count; i++)
        {
            var vertexData = roomBuilder.vertices[i];
            Transform vertexTransform = roomBuilder.transform.Find(vertexData.name);
            if (vertexTransform == null)
                continue;

            Handles.color = Color.white;

            if (vertexTransform == selectedTransform)
            {
                Handles.color = Color.yellow; // Highlight selected vertex
            }

            EditorGUI.BeginChangeCheck();

            Vector3 worldPos = vertexTransform.position;

            // Display a position handle for each vertex
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(roomBuilder, "Move Vertex");
                Vector3 localPos = roomBuilder.transform.InverseTransformPoint(newWorldPos);
                vertexData.localPosition = localPos;
                vertexTransform.localPosition = localPos;
                roomBuilder.UpdateRoom();
                serializedObject.Update(); // Update serialized properties
            }
        }
    }

    private void OnUndoRedoPerformed()
    {
        if (roomBuilder == null)
        {
            roomBuilder = (RoomBuilder)target;
        }

        roomBuilder.ReconstructVertexTransforms();
        roomBuilder.UpdateRoom();
        SceneView.RepaintAll();
    }
}
