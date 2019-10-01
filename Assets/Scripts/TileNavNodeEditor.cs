using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileNavNode))]
[CanEditMultipleObjects]
public class TileNavNodeEditor : Editor {
    SerializedProperty thisNodesType;
    SerializedProperty comeToStop;
    SerializedProperty nextNodeLinker;
    SerializedProperty nextNode;
    SerializedProperty prevNodeLinker;
    SerializedProperty prevNode;

    SerializedProperty nextNodeArrayLength;
    SerializedProperty prevNodeArrayLength;


    void OnEnable() {
        thisNodesType = serializedObject.FindProperty("thisNodesType");
        comeToStop = serializedObject.FindProperty("comeToStop");
        nextNodeLinker = serializedObject.FindProperty("nextNodeLinker");
        nextNode = serializedObject.FindProperty("nextNode");
        nextNodeArrayLength = nextNode.FindPropertyRelative("Array.size");
        prevNodeLinker = serializedObject.FindProperty("prevNodeLinker");
        prevNode = serializedObject.FindProperty("prevNode");
        prevNodeArrayLength = prevNode.FindPropertyRelative("Array.size");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(thisNodesType);
        EditorGUILayout.PropertyField(comeToStop);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(nextNodeLinker);
        if (nextNodeLinker.intValue == 0) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(nextNodeArrayLength);
            for (int i = 0; i < nextNodeArrayLength.intValue; i++) {
                EditorGUILayout.PropertyField(nextNode.GetArrayElementAtIndex(i));
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(prevNodeLinker);
        if (prevNodeLinker.intValue == 0) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(prevNodeArrayLength);
            for (int i = 0; i < prevNodeArrayLength.intValue; i++) {
                EditorGUILayout.PropertyField(prevNode.GetArrayElementAtIndex(i));
            }
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}