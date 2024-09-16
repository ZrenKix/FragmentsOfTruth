using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RoomBuilder))]
public class RoomBuilderEditor : Editor
{
    private RoomBuilder roomBuilder;
    private bool showVerticesList = true;
    private SerializedProperty wallsProp;

    private List<bool> wallFoldouts = new List<bool>();

    private void OnEnable()
    {
        roomBuilder = (RoomBuilder)target;
        wallsProp = serializedObject.FindProperty("walls");

        InitializeWallFoldouts();
    }

    void InitializeWallFoldouts()
    {
        wallFoldouts.Clear();
        int numWalls = roomBuilder != null && roomBuilder.walls != null ? roomBuilder.walls.Count : 0;
        for (int i = 0; i < numWalls; i++)
        {
            wallFoldouts.Add(false);
        }
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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("floorMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingMaterial"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingHeight"));

        // Display and edit vertex positions
        showVerticesList = EditorGUILayout.Foldout(showVerticesList, "Floor Vertices");
        if (showVerticesList)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < roomBuilder.vertices.Count; i++)
            {
                var vertexData = roomBuilder.vertices[i];
                Transform vertexTransform = vertexData.transform;

                EditorGUILayout.BeginHorizontal();

                // Display and edit the name of the vertex
                string oldName = vertexData.name;
                string newName = EditorGUILayout.TextField(oldName, GUILayout.Width(100));

                if (newName != oldName)
                {
                    Undo.RecordObject(vertexTransform.gameObject, "Rename Vertex");
                    vertexData.name = newName;
                    vertexTransform.gameObject.name = newName;
                }

                // Display and edit the localPosition of the vertex
                Vector3 oldPosition = vertexTransform.localPosition;
                Vector3 newPosition = EditorGUILayout.Vector3Field("", oldPosition);

                if (newPosition != oldPosition)
                {
                    Undo.RecordObject(vertexTransform, "Move Vertex");
                    vertexTransform.localPosition = newPosition;
                    roomBuilder.UpdateRoom();
                    serializedObject.Update(); // Update serialized properties
                }

                // Insert Vertex After
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    Undo.RecordObject(roomBuilder, "Insert Vertex");
                    Vector3 positionA = vertexTransform.localPosition;
                    Vector3 positionB = roomBuilder.vertices[(i + 1) % roomBuilder.vertices.Count].transform.localPosition;
                    Vector3 newVertexPosition = (positionA + positionB) / 2f;
                    string newVertexName = "Vertex_" + roomBuilder.vertices.Count;
                    roomBuilder.InsertVertex(i + 1, newVertexPosition, newVertexName);
                    InitializeWallFoldouts();
                    roomBuilder.UpdateRoom();
                    serializedObject.Update(); // Update serialized properties
                    break; // Break to avoid modifying collection during iteration
                }

                // Remove Vertex
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    if (roomBuilder.vertices.Count > 3)
                    {
                        Undo.RecordObject(roomBuilder, "Remove Vertex");
                        roomBuilder.RemoveVertex(i);
                        InitializeWallFoldouts();
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

            // Note: The Add Vertex and Remove Last Vertex buttons are optional now.
            // You can remove them or keep them based on your preference.
        }

        // Synchronize wallFoldouts with the number of walls
        if (wallFoldouts.Count != roomBuilder.walls.Count)
        {
            InitializeWallFoldouts();
        }

        // Display wall settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Walls", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        int wallsCount = Mathf.Min(wallsProp.arraySize, roomBuilder.vertices.Count);

        for (int i = 0; i < wallsCount; i++)
        {
            SerializedProperty wallProp = wallsProp.GetArrayElementAtIndex(i);
            SerializedProperty isVisibleProp = wallProp.FindPropertyRelative("isVisible");
            SerializedProperty openingsProp = wallProp.FindPropertyRelative("openings");

            int nextIndex = (i + 1) % roomBuilder.vertices.Count;
            string vertexNameA = roomBuilder.vertices[i].name;
            string vertexNameB = roomBuilder.vertices[nextIndex].name;

            string wallLabel = $"Wall {i} ({vertexNameA} - {vertexNameB})";

            // Ensure wallFoldouts has enough elements
            if (wallFoldouts.Count <= i)
            {
                wallFoldouts.Add(false);
            }

            wallFoldouts[i] = EditorGUILayout.Foldout(wallFoldouts[i], wallLabel);
            if (wallFoldouts[i])
            {
                EditorGUI.indentLevel++;

                // Wall visibility
                isVisibleProp.boolValue = EditorGUILayout.Toggle("Visible", isVisibleProp.boolValue);

                // Openings
                SerializedProperty openingsArray = openingsProp;
                int openingsCount = openingsArray.arraySize;

                EditorGUILayout.LabelField("Openings");
                EditorGUI.indentLevel++;

                for (int j = 0; j < openingsCount; j++)
                {
                    SerializedProperty openingProp = openingsArray.GetArrayElementAtIndex(j);
                    SerializedProperty positionProp = openingProp.FindPropertyRelative("position");
                    SerializedProperty widthProp = openingProp.FindPropertyRelative("width");
                    SerializedProperty heightProp = openingProp.FindPropertyRelative("height");
                    SerializedProperty bottomProp = openingProp.FindPropertyRelative("bottom");

                    EditorGUILayout.LabelField($"Opening {j + 1}");
                    EditorGUI.indentLevel++;

                    positionProp.floatValue = EditorGUILayout.FloatField("Position", positionProp.floatValue);
                    widthProp.floatValue = EditorGUILayout.FloatField("Width", widthProp.floatValue);
                    heightProp.floatValue = EditorGUILayout.FloatField("Height", heightProp.floatValue);
                    bottomProp.floatValue = EditorGUILayout.FloatField("Bottom", bottomProp.floatValue);

                    // Remove opening button
                    if (GUILayout.Button("Remove Opening"))
                    {
                        openingsArray.DeleteArrayElementAtIndex(j);
                        break;
                    }

                    EditorGUI.indentLevel--;
                }

                // Add opening button
                if (GUILayout.Button("Add Opening"))
                {
                    openingsArray.arraySize++;
                    SerializedProperty newOpeningProp = openingsArray.GetArrayElementAtIndex(openingsCount);
                    newOpeningProp.FindPropertyRelative("position").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("width").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("height").floatValue = 1f;
                    newOpeningProp.FindPropertyRelative("bottom").floatValue = 0f;
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
        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < roomBuilder.vertices.Count; i++)
        {
            Transform vertexTransform = roomBuilder.vertices[i].transform;
            Vector3 worldPos = vertexTransform.position;

            // Display a position handle for each vertex
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vertexTransform, "Move Vertex");
                vertexTransform.position = newWorldPos;
                roomBuilder.UpdateRoom();
                serializedObject.Update(); // Update serialized properties
            }
        }
    }
}
