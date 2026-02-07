using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShaderAnimData))]
public class ShaderAnimDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        var data = (ShaderAnimData)target;

        if (GUILayout.Button("Auto Calculate from Sprites", GUILayout.Height(30)))
        {
            Undo.RecordObject(data, "Auto Calculate Animation Data");
            data.AutoCalculateFromSprites();
            EditorUtility.SetDirty(data);
        }
    }
}