using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UniRx;
using UnityAtoms.BaseAtoms;

public class PlanetMesh : TextureToSphere
{
    [Header("PlanetMesh")]
    [SerializeField]
    private MeshFilter planet;
    [SerializeField]
    private MeshFilter flatPlanet;

    public override void Awake()
    {
        base.Awake();
        flatPoints.Subscribe(points =>
        {
            Debug.Log("triangle");
            Triangulate(points, out List<Vector3> verts, out List<int> indices);
            Debug.Log("triangle done");

            var psPoints = new ComputeBuffer(verts.Count + 16, sizeof(float) * 3);

            psPoints.SetData(verts);

            var id = shader.FindKernel("PlaneToSphere");
            shader.SetBuffer(id, "psPoints", psPoints);
            shader.Dispatch(id, verts.Count / 16 + 1, 1, 1);

            var p = new Vector3[verts.Count];
            psPoints.GetData(p);

            this.colors.SetValueAndForceNotify(ColorToSphere(new List<Vector3>(p)));

            var colors = new List<Color>();
            for (int i = 0; i < p.Length; i++)
            {
                p[i] *= radius.Value + heightMultiplier.Value * this.colors.Value[i].x;
                colors.Add(this.colors.Value[i]);
            }
            planet.mesh.Clear();
            planet.mesh.vertices = p;
            planet.mesh.triangles = indices.ToArray();
            planet.mesh.Optimize();
            planet.mesh.RecalculateNormals();


            for (int i = 0; i < verts.Count; i++)
            {
                verts[i] *= radius.Value;
            }
            flatPlanet.mesh.Clear();
            flatPlanet.mesh.vertices = verts.ToArray();
            flatPlanet.mesh.triangles = indices.ToArray();
            flatPlanet.mesh.Optimize();
            flatPlanet.mesh.RecalculateNormals();
        });
    }

    public void Triangulate(Vector3[] points, out List<Vector3> vertices, out List<int> indices)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        var p = new Polygon();

        for (int i = 0; i < points.Length; i++)
        {
            p.Add(new Vertex(points[i].x, points[i].y));
        }

        float subDivisions = Mathf.Sqrt(points.Length);

        for (int i = 0; i < subDivisions; i++)
        {
            var height = (float)Mathf.Lerp(0, 1, i / subDivisions);
            p.Add(new Vertex(0, height));
            p.Add(new Vertex(2, height));
        }

        var contour = new List<Vertex>();

        contour.Add(new Vertex(0, 0));
        contour.Add(new Vertex(0, 1));
        contour.Add(new Vertex(2, 1));
        contour.Add(new Vertex(2, 0));

        p.Add(new Contour(contour), false);

        var options = new ConstraintOptions();
        options.ConformingDelaunay = false;
        options.Convex = true;

        var quality = new QualityOptions();
        quality.MinimumAngle = 0;
        quality.MaximumAngle = 120;

        var mesh = p.Triangulate(options, quality);

        foreach (ITriangle t in mesh.Triangles)
        {
            {
                for (int j = 2; j >= 0; j--)
                {
                    bool found = false;
                    for (int k = 0; k < vertices.Count; k++)
                    {
                        if (vertices[k].x == t.GetVertex(j).X && vertices[k].y == t.GetVertex(j).Y)
                        {
                            indices.Add(k);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        vertices.Add(new Vector3((float)t.GetVertex(j).X, (float)t.GetVertex(j).Y, 0));
                        indices.Add(vertices.Count - 1);
                    }
                }
            }
        }
    }
}
