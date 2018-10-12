using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangulator {

    public static List<int> Triangulate(List<Vector3> points, bool clockwise = true)
    {
        List<int> tris = new List<int>();
        if (points.Count < 3)
        {
            return tris;
        }
        List<Vector3> copyPoints = new List<Vector3>(points);
        List<int> indices = new List<int>(points.Count);
        int i; // also used later
        for (i = 0; i < points.Count; i++)
        {
            indices.Add(i);
        }
        //Debug.Log("Start reducing points");
        i = 0;
        int a, b, c;
        int previousCount = points.Count;
        while (copyPoints.Count > 2)
        {
            // test to see if you can make an angle and have it be clockwise! If so, add them to the list, and then remove the middle point.
            a = i % copyPoints.Count;
            b = (i + 1) % copyPoints.Count;
            c = (i + 2) % copyPoints.Count;

            if (IsClockwiseAngle(copyPoints[a], copyPoints[b], copyPoints[c]) == clockwise)
            {
                //Debug.Log(copyPoints.Count + ": a " + a + " b " + b + " c " + c);
                // then add the triangle to the list of triangles
                // then check to see if any point is inside
                bool valid = true;
                for (int j = 0; j < copyPoints.Count - 3; j++)
                {
                    // test the points after c to see if any of those points are inside the triangle. If so, it's not valid
                    if (IsPointInside(copyPoints[c], copyPoints[b], copyPoints[a], copyPoints[(c+1+j) % copyPoints.Count]))
                    {
                        valid = false;
                        i++;
                        break;
                    }
                }
                if (valid)
                {
                    // then we can remove this point and create a triangle!
                    tris.Add(indices[a]);
                    tris.Add(indices[b]);
                    tris.Add(indices[c]);
                    copyPoints.RemoveAt(b); // take away the middle "sticky outy" point
                    indices.RemoveAt(b);
                }
            } else
            {
                i++;
            }

            if (b == 0)
            {
                // then we've made a full loop
                if (copyPoints.Count == previousCount)
                {
                    // then we made no progress, so kill it.
                    Debug.LogError("Failed to build a list of tris. Tris found: " + (tris.Count / 3));
                    return tris;
                }
                previousCount = copyPoints.Count;
            }
        }
        return tris; // may need to be reversed depending on what clockwise actually is...
    }

    public static bool IsClockwiseAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        // angle between a-b-c
        // like so:
        // b-a
        // |
        // c
        // returns if it's clockwise or not I guess...

        Vector3 ba = a - b;
        Vector3 bc = c - b;
        return Vector3.SignedAngle(ba, bc, Vector3.forward) >= 0;
    }

    public static bool IsPointInside(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        // this is the barrycentric method. It also assumes it's a 2d Triangle
        Vector2 v0 = c - a;
        Vector2 v1 = b - a;
        Vector2 v2 = p - a;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float inverseDenominator = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * inverseDenominator;
        float v = (dot00 * dot12 - dot01 * dot02) * inverseDenominator;
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
	
}
