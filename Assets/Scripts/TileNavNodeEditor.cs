using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileNavNode))]
[CanEditMultipleObjects]
public class TileNavNodeEditor : Editor {
    SerializedProperty thisNodesType;
    SerializedProperty comeToStop;
    SerializedProperty nextTileLinker;
    SerializedProperty nextNode;
    SerializedProperty prevTileLinker;
    SerializedProperty prevNode;

    SerializedProperty nextNodeArrayLength;
    SerializedProperty prevNodeArrayLength;

    GUIStyle guiStyle;


    void OnEnable() {
        thisNodesType = serializedObject.FindProperty("thisNodesType");
        comeToStop = serializedObject.FindProperty("comeToStop");
        nextTileLinker = serializedObject.FindProperty("nextTileLinker");
        nextNode = serializedObject.FindProperty("nextNode");
        nextNodeArrayLength = nextNode.FindPropertyRelative("Array.size");
        prevTileLinker = serializedObject.FindProperty("prevTileLinker");
        prevNode = serializedObject.FindProperty("prevNode");
        prevNodeArrayLength = prevNode.FindPropertyRelative("Array.size");

    }

    public override void OnInspectorGUI() {
        guiStyle = GUI.skin.textArea;

        serializedObject.Update();
        EditorGUILayout.PropertyField(thisNodesType);
        EditorGUILayout.PropertyField(comeToStop);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(nextTileLinker);
        if (nextTileLinker.intValue == 0) {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(guiStyle);
            EditorGUILayout.PropertyField(nextNodeArrayLength);
            for (int i = 0; i < nextNodeArrayLength.intValue; i++) {
                EditorGUILayout.PropertyField(nextNode.GetArrayElementAtIndex(i));
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(prevTileLinker);
        if (prevTileLinker.intValue == 0) {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(guiStyle);
            EditorGUILayout.PropertyField(prevNodeArrayLength);
            for (int i = 0; i < prevNodeArrayLength.intValue; i++) {
                EditorGUILayout.PropertyField(prevNode.GetArrayElementAtIndex(i));
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}