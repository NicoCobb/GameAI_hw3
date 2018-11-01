using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "Path Follow Behavior", menuName = "Behaviors/Path", order = 0)]
public class PathFollowingBehaviorBAD : Behavior
{
    public List<Vector3> points = new List<Vector3>(); // the path to follow
    public float predictionTime = 1;

    public override SteeringOutput Action(Transform position, Transform targetPos, SteeringOutput prev, float maxAcceleration)
    {
        // this is predictive path following
        float speed = position.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        Vector3 futureTarget = position.position + (Vector3)(predictionTime * position.gameObject.GetComponent<Rigidbody2D>().velocity);
        // this ^ is where the character will be in predictionTime seconds if they don't change their velocity etc.

        Part3Boid boid = position.gameObject.GetComponent<Part3Boid>();

        float currentPathProgress = boid.pathProgress; // use the previously stored path progress, then update it at the end
        // Vector3 currentPathTarget = GetClosestPoint(position.position, out currentPathProgress);

        // get the path progress of your current point, so that we know what counts as moving along the path in this next function call:
        // (basically this is to ensure that we don't backtrack along the path)
        float futurePathProgress;
        Debug.Log(currentPathProgress);
        Vector3 futurePathTarget = GetClosestPointPastProgress(currentPathProgress + .1f, futureTarget, out futurePathProgress);
        Debug.DrawLine(position.position, futurePathTarget);
        // then path towards this new position that is further along the line than we currently are and is close to our future position.
        boid.pathProgress = futurePathProgress;
        return new SteeringOutput(VectorAngle(futurePathTarget - position.position), maxAcceleration);
    }

    public override string GetName()
    {
        return "Predictive Path Follow";
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

    public void SetPoints(List<Vector3> pts)
    {
        points = pts;
    }

    public Vector3 GetClosestPointPastProgress(float currentProgress, Vector3 pos, out float pathProgress)
    {
        if (points.Count < 2)
        {
            // then you can't do it because the line is screwed up so return your own position
            pathProgress = 0;
            return pos;
        }
        float minDistance = float.MaxValue; // it had better find some distance smaller than this //  Vector3.Distance(pos, points[points.Count - 1]);
        Vector3 minPos = points[points.Count - 1];
        float distanceAlongPath = 0;
        // gets the point closest to the path
        float currDistanceAlongPath = 0;
        int numSegmentsToCheck = 2; // only check the segment you're on and one more after that
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (currDistanceAlongPath <= currentProgress)
            {
                currDistanceAlongPath += Vector3.Distance(points[i], points[i + 1]);
                continue; // don't check it if it's not at least equivalent to the current progress to ensure that we don't move backwards.
            } else
            {
                // then you get two whole line segments.
                numSegmentsToCheck--;
                Debug.Log("Here");
                if (numSegmentsToCheck <= 0)
                {
                    // then exit
                    break;
                }
            }
            Vector3 curr = ClosestPointToLineSegment(pos, points[i], points[i + 1]);
            float currDistance = Vector3.Distance(curr, pos);
            if (currDistance < minDistance)
            {
                minDistance = currDistance;
                minPos = curr;
                distanceAlongPath = currDistanceAlongPath + Vector3.Distance(points[i], minPos);
                pathProgress = distanceAlongPath;
                return minPos;
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
