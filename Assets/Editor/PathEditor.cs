using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(PathScript))]
public class PathEditor : Editor {

    public override void OnInspectorGUI()
    {
        PathScript path = (PathScript)target;

        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            // in case they added something or edited something
            path.UpdatePoints();
        }
    }
    private void OnSceneGUI()
    {
        PathScript path = (PathScript)target;
        Vector3 pos = path.transform.position;

        EditorGUI.BeginChangeCheck();

        // check if someone is pressing the p key if so add another point
        if (EditorWindow.focusedWindow)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
            {
                if (path.points.Count > 0)
                {
                    path.points.Add(path.points[path.points.Count - 1]);
                }
                else
                {
                    path.points.Add(Vector3.zero);
                }
            }
        }


        for (int i = 0; i < path.points.Count; i++)
        {
            path.points[i] = Handles.PositionHandle(pos + (Vector3)path.points[i], Quaternion.identity) - pos;
            /*if (i > 0)
            {
                Debug.DrawLine(path.points[i] + pos, path.points[i - 1] + pos);
            }*/
        }

        if (EditorGUI.EndChangeCheck())
        {
            path.UpdatePoints();
        }
    }
}
