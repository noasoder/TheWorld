using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesh))]
public class PlanetMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Planet"))
        {
            if (this.target is PlanetMesh tile)
            {
                tile.Generate();
            }
        }
    }
}
