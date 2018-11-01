using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Path Follow Behavior", menuName = "Behaviors/Path", order = 0)]
public class PathFollowingBehavior : Behavior
{
    public List<Vector3> points = new List<Vector3>(new Vector3[]{ new Vector3(0, -20, 0), new Vector3(3, -15, 0), new Vector3(10, -12, 0), new Vector3(15, -7, 0), new Vector3(10, -2, 0), new Vector3(2, 0, 0), new Vector3(-5, -6, 0), new Vector3(-7, -5, 0), new Vector3(-9, 0, 0), new Vector3(0, 10, 0), new Vector3(0, 15, 0)}); // the path to follow
    public float predictionTime = 1;
    public float distanceGoal = .5f; // the distance to the point on the path before it moves onto the next item of the path.

    public float frontRaycastLength = 5;
    public float avoidDistance = 3;
    public float feeler1Angle = 45;
    public float feeler1Length = .5f;

    [Space]
    public LayerMask feelerLayermask;

    private bool sidetracked = false; // if you hit a wall then become sidetracked and follow the side whiskers.

    public override SteeringOutput Action(Transform position, Transform targetPos, SteeringOutput prev, float maxAcceleration)
    {
        // this is predictive path following
        float speed = position.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude;
        Vector3 futureTarget = position.position + (Vector3)(predictionTime * position.gameObject.GetComponent<Rigidbody2D>().velocity);
        // this ^ is where the character will be in predictionTime seconds if they don't change their velocity etc.

        Part3Boid boid = position.gameObject.GetComponent<Part3Boid>();

        int pathProgress = boid.pathTargetIndex; // the index of the point on the path you're heading to
        Vector3 target = points[pathProgress];
        if (Vector3.Distance(position.position, target) < distanceGoal)
        {
            boid.pathTargetIndex = Mathf.Min(boid.pathTargetIndex + 1, points.Count - 1); // so that we don't error at the end.
        }

        // Debug.DrawLine(position.position, target);
        RaycastHit2D centerHit = Physics2D.Raycast(position.position, position.right, frontRaycastLength, feelerLayermask.value);
        Vector2 rightAngle = Quaternion.Euler(0, 0, -feeler1Angle) * position.right;
        Vector2 leftAngle = Quaternion.Euler(0, 0, feeler1Angle) * position.right;
        RaycastHit2D leftHit = Physics2D.Raycast(position.position, leftAngle, feeler1Length, feelerLayermask.value);
        RaycastHit2D rightHit = Physics2D.Raycast(position.position, rightAngle, feeler1Length, feelerLayermask.value);
        if (centerHit)
        {
            Debug.DrawLine(position.position, centerHit.point);
            if (!sidetracked)
            {
                Debug.Log("Sidetracked");
            }
            sidetracked = true;
            if (!leftHit && !rightHit)
            {
                Debug.Log("Traveling via normal");
                Debug.DrawRay((centerHit.point + centerHit.normal * avoidDistance), prev.GetDPos());
                return new SteeringOutput(VectorAngle((centerHit.point + centerHit.normal * avoidDistance) - (Vector2)position.position), maxAcceleration);
            }
            //target += (Vector3)hit.normal;
        }

        if (sidetracked || true)
        {
            // then follow the whiskers if they're hitting something
            bool hitSomething = false;
            if (leftHit && rightHit)
            {
                // then center your avoidance between them so you can navigate tunnels?
                Debug.Log("HIT BOTH");
                Vector2 hitPos = (leftHit.point + leftHit.normal * avoidDistance + rightHit.point + rightHit.normal * avoidDistance) / 2;
                Debug.DrawRay(hitPos, prev.GetDPos());
                Debug.DrawLine(position.position, leftHit.point);
                Debug.DrawLine(position.position, rightHit.point);
                return new SteeringOutput(VectorAngle(hitPos - (Vector2)position.position), maxAcceleration);
            }
            else
            {
                if (leftHit)
                {
                    Debug.Log("Hit left");
                    hitSomething = true;
                    Debug.DrawLine(position.position, leftHit.point);
                    Debug.DrawRay((leftHit.point + leftHit.normal * avoidDistance), prev.GetDPos());
                    return new SteeringOutput(VectorAngle((leftHit.point + leftHit.normal * avoidDistance) - (Vector2)position.position), maxAcceleration);
                }
                else
                {
                    Debug.DrawLine(position.position, position.position + (Vector3)leftAngle * feeler1Length);
                }
                if (rightHit)
                {
                    Debug.Log("Hit right");
                    hitSomething = true;
                    Debug.DrawRay((rightHit.point + rightHit.normal * avoidDistance), prev.GetDPos());
                    Debug.DrawLine(position.position, rightHit.point);
                    return new SteeringOutput(VectorAngle((rightHit.point + rightHit.normal * avoidDistance) - (Vector2)position.position), maxAcceleration);
                }
                else
                {
                    Debug.DrawLine(position.position, position.position + (Vector3)rightAngle * feeler1Length);
                }

                if (!hitSomething)
                {
                    sidetracked = false;
                    Debug.Log("Not Sidetracked");
                }
            }
        }
        else if (!sidetracked)
        {
            Debug.DrawLine(position.position, position.position + position.right * frontRaycastLength);
        }
        // for now just test by pointing yourself towards the point and heading that direction since we don't have collisions.
        return new SteeringOutput(VectorAngle(target - position.position), maxAcceleration);
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
