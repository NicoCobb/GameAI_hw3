using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "Path Follow Behavior", menuName = "Behaviors/Path", order = 0)]
public class PathFollowingBehaviorRidingHoodVersion : Behavior
{
    public List<Vector3> points = new List<Vector3>(new Vector3[]{ new Vector3(0, -20, 0), new Vector3(3, -15, 0), new Vector3(10, -12, 0), new Vector3(15, -7, 0), new Vector3(10, -2, 0), new Vector3(2, 0, 0), new Vector3(-5, -6, 0), new Vector3(-7, -5, 0), new Vector3(-9, 0, 0), new Vector3(0, 10, 0), new Vector3(0, 15, 0)}); // the path to follow
    public float predictionTime = 1;

    public override SteeringOutput Action(Transform position, Transform targetPos, SteeringOutput prev, float maxAcceleration)
    {
        // this is predictive path following
        float speed = position.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        Vector3 futureTarget = position.position + (Vector3)(predictionTime * position.gameObject.GetComponent<Rigidbody2D>().velocity);
        // this ^ is where the character will be in predictionTime seconds if they don't change their velocity etc.

        float currentPathProgress;
        Vector3 currentPathTarget = GetClosestPoint(position.position, out currentPathProgress);
        // get the path progress of your current point, so that we know what counts as moving along the path in this next function call:
        // (basically this is to ensure that we don't backtrack along the path)
        float futurePathProgress;
        Vector3 futurePathTarget = GetClosestPointPastProgress(currentPathProgress + .1f, futureTarget, out futurePathProgress);
        // then path towards this new position that is further along the line than we currently are and is close to our future position.

        Debug.DrawLine(position.position, futurePathTarget);

        return new SteeringOutput(VectorAngle(futurePathTarget - position.position), maxAcceleration);
    }

    public override string GetName()
    {
        return "Predictive Path Follow";
    }

    public void SetPoints(List<Vector3> pts)
    {
        points = pts;
    }

    public Vector3 GetClosestPoint(Vector3 pos, out float pathProgress)
    {
        if (points.Count < 2)
        {
            // then you can't do it because the line is screwed up so return your own position
            pathProgress = 0;
            return pos;
        }
        float minDistance = Vector3.Distance(pos, points[0]);
        Vector3 minPos = points[0];
        float distanceAlongPath = 0;
        // gets the point closest to the path
        float currDistanceAlongPath = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 curr = ClosestPointToLineSegment(pos, points[i], points[i + 1]);
            float currDistance = Vector3.Distance(curr, pos);
            if (currDistance < minDistance)
            {
                minDistance = currDistance;
                minPos = curr;
                distanceAlongPath = currDistanceAlongPath + Vector3.Distance(points[i], minPos);
            }
            currDistanceAlongPath += Vector3.Distance(points[i], points[i + 1]);
        }
        pathProgress = distanceAlongPath;
        return minPos;
    }

    public Vector3 GetClosestPointPastProgress(float currentProgress, Vector3 pos, out float pathProgress)
    {
        if (points.Count < 2)
        {
            // then you can't do it because the line is screwed up so return your own position
            pathProgress = 0;
            return pos;
        }
        float minDistance = Vector3.Distance(pos, points[points.Count - 1]);
        Vector3 minPos = points[points.Count - 1];
        float distanceAlongPath = 0;
        // gets the point closest to the path
        float currDistanceAlongPath = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (currDistanceAlongPath <= currentProgress)
            {
                currDistanceAlongPath += Vector3.Distance(points[i], points[i + 1]);
                continue; // don't check it if it's not at least equivalent to the current progress to ensure that we don't move backwards.
            }
            Vector3 curr = ClosestPointToLineSegment(pos, points[i], points[i + 1]);
            float currDistance = Vector3.Distance(curr, pos);
            if (currDistance < minDistance)
            {
                minDistance = currDistance;
                minPos = curr;
                distanceAlongPath = currDistanceAlongPath + Vector3.Distance(points[i], minPos);
            }
            currDistanceAlongPath += Vector3.Distance(points[i], points[i + 1]);
        }
        pathProgress = distanceAlongPath;
        return minPos;
    }

    private Vector3 ClosestPointToLineSegment(Vector3 pos, Vector3 a, Vector3 b)
    {
        // closest point from pos to the line segment denoted by ab
        float segmentLength = Vector3.Distance(a, b);
        Vector3 linedir = (b - a).normalized;
        float projected = Vector3.Dot(pos - a, linedir);
        //float lineDistance = Vector3.Distance(linepoint, pos);
        if (projected <= 0)
        {
            return a;
        }
        else if (projected >= segmentLength)
        {
            return b;
        }
        else
        {
            Vector3 linepoint = a + linedir * projected;
            return linepoint;
        }
    }
}
