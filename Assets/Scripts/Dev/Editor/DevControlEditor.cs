using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DevControl))]
public class DevControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DevControl dev = (DevControl)target;
        Transform moved = dev.target != null ? dev.target : dev.transform;

        EditorGUILayout.Space();

        if (GUILayout.Button("Move To Custom Position"))
            Run(moved, dev.MoveToCustomPosition);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("TP Map 1"))
            Run(moved, dev.TeleportToMap1);
        if (GUILayout.Button("TP Map 2"))
            Run(moved, dev.TeleportToMap2);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Use Current Position As Custom"))
        {
            Undo.RecordObject(dev, "Set Custom Position");
            dev.customPosition = moved.position;
        }
    }

    static void Run(Transform moved, System.Action action)
    {
        // Makes edit-mode moves undoable with Ctrl+Z and marks the scene dirty.
        Undo.RecordObject(moved, "Dev Teleport");
        action();
    }
}
