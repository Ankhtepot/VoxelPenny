using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinGrapher))]
public class PerlinGrapherEditor : Editor
{
    void OnSceneGUI()
    {
        PerlinGrapher handle = (PerlinGrapher)target;
        if (!handle)
        {
            return;
        }

        Handles.color = Color.blue;
        Handles.Label(handle.lr.GetPosition(0) + Vector3.up * 2,
            "Layer: " +
            handle.gameObject.name);
    }

}
