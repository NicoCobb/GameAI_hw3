using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Terrain2D))]
public class TerrainEditor : Editor {

    public override void OnInspectorGUI()
    {
        Terrain2D terrain = (Terrain2D)target;

        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            // in case they added something or edited something
            terrain.UpdatePoints();
        }
    }

    private void OnSceneGUI()
    {
        Terrain2D terrain = (Terrain2D)target;
        Vector3 pos = terrain.transform.position;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < terrain.points.Count; i++)
        {
            terrain.points[i] = Handles.PositionHandle(pos + (Vector3)terrain.points[i], Quaternion.identity) - pos;
            /*if (i > 0)
            {
                Debug.DrawLine(terrain.points[i] + pos, terrain.points[i - 1] + pos);
            }*/
        }

        if (EditorGUI.EndChangeCheck())
        {
            terrain.UpdatePoints();
        }
    }
}
