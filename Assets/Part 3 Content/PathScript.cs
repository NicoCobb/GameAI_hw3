using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathScript : MonoBehaviour {
    public List<Vector3> points = new List<Vector3>();

    [SerializeField]
    private List<PathFollowingBehavior> pathFollowingBehaviors; // to set the path values for these paths

    [ContextMenu("Update points")]
    public void UpdatePoints()
    {
        // this gets called to update the linerenderer in the editor
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        foreach(PathFollowingBehavior p in pathFollowingBehaviors)
        {
            p.SetPoints(points);
        }
    }
}
