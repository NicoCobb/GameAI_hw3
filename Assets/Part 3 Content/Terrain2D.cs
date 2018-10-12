using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
public class Terrain2D : MonoBehaviour {

    public List<Vector3> points = new List<Vector3>();
    public bool isClockwise = true;

    public void UpdatePoints()
    {
        // this is being run in the editor unless we change points and update it for some other reason...

        List<Vector2> polyPoints = new List<Vector2>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            polyPoints.Add(points[i]);
        }
        GetComponent<PolygonCollider2D>().SetPath(0, polyPoints.ToArray());

        // then set the mesh

        List<int> tris = MeshTriangulator.Triangulate(points, isClockwise);
        if (!isClockwise)
        {
            tris.Reverse(); // otherwise the normals face the wrong way...
        }
        // Debug.Log("Found " + (tris.Count/3) + " triangles");
        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, polyPoints); // use the 2d points calculated before
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
