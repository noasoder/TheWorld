using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UniRx;
using UnityAtoms.BaseAtoms;

public class PlanetMesh : TextureToSphere
{
    [Header("PlanetMesh")]
    [SerializeField]
    private MeshFilter flatPlanet;

    [SerializeField]
    private string planetName;
    [SerializeField]
    private Material planetMaterial;

    [SerializeField]
    private BoolReference overrideSavedMesh;

    [SerializeField]
    private List<MeshFilter> models;

    private string savePath = "Assets/TheWorld/GeneratedPlanets/";

    public void GenerateMesh()
    {
        var savePath = this.savePath + this.planetName;
        var filepath = savePath + ".asset";

        var points = GetUVs(pointSphere.GeneratePoints());

        models = new List<MeshFilter>(transform.GetComponentsInChildren<MeshFilter>());
        for (int i = 0; i < 6; i++)
        {
            if(models.Count <= i)
            {
                var newModel = new GameObject("Model " + i);
                newModel.transform.SetParent(transform, false);
                models.Add(newModel.AddComponent<MeshFilter>());
                var meshRenderer = newModel.AddComponent<MeshRenderer>();
                meshRenderer.material = planetMaterial;
            }

            Triangulate(points[0].ToArray(), out List<Vector2> verts, out List<int> indices);

            //Compute
            var pPoints = new ComputeBuffer(verts.Count, sizeof(float) * 2);
            var sPoints = new ComputeBuffer(verts.Count, sizeof(float) * 3);

            pPoints.SetData(verts);

            var id = shader.FindKernel("PlaneToSphere");
            shader.SetBuffer(id, "pPoints", pPoints);
            shader.SetBuffer(id, "sPoints", sPoints);

            shader.Dispatch(id, verts.Count, 1, 1);

            var p = new Vector3[verts.Count];
            sPoints.GetData(p);
            pPoints.Release();
            sPoints.Release();

            List<List<Vector2>> uvs = new List<List<Vector2>>();
            if (i <= 3)
            {
                var data = new List<List<Vector3>>();
                data.Add(new List<Vector3>(p));
                uvs = GetUVs(data, i <= 3 ? i * -0.25f : 0);

                //Rotate
                var rotation = new Vector3(0, i <= 3 ? i * 90 : 0, 0);
                for (int j = 0; j < p.Length; j++)
                {
                    p[j] = (Quaternion.Euler(rotation) * p[j]);
                }
            }
            else
            {
                //Rotate
                var rotation = new Vector3(i == 4 ? 90 : i == 5 ? -90 : 0, 0, 0);
                for (int j = 0; j < p.Length; j++)
                {
                    p[j] = (Quaternion.Euler(rotation) * p[j]);
                }

                var data = new List<List<Vector3>>();
                data.Add(new List<Vector3>(p));
                uvs = GetUVs(data, 0, i == 4 ? 0.5f : 0);
            }

            var colors = (ColorToSphere(new List<Vector3>(p)));

            for (int j = 0; j < p.Length; j++)
            {
                p[j] *= radius.Value + heightMultiplier.Value * colors[j].x;
            }
            models[i].sharedMesh = new Mesh();
            models[i].sharedMesh.Clear();
            models[i].sharedMesh.vertices = p;
            models[i].sharedMesh.triangles = indices.ToArray();
            models[i].sharedMesh.SetUVs(0, uvs[0]);
            models[i].sharedMesh.Optimize();
            models[i].sharedMesh.RecalculateBounds();
            models[i].sharedMesh.RecalculateTangents();
            models[i].sharedMesh.RecalculateNormals();

            var modelPath = savePath + i + ".asset";
            AssetDatabase.DeleteAsset(modelPath);
            AssetDatabase.CreateAsset(models[i].sharedMesh, modelPath);

            //Flat mesh
            var vec2 = new List<Vector3>();
            for (int j = 0; j < verts.Count; j++)
            {
                verts[j] *= radius.Value;
                vec2.Add(verts[j]);
            }
            flatPlanet.sharedMesh.Clear();
            flatPlanet.sharedMesh.vertices = vec2.ToArray();
            flatPlanet.sharedMesh.triangles = indices.ToArray();
            flatPlanet.sharedMesh.Optimize();
            flatPlanet.sharedMesh.RecalculateNormals();
        }
    }

    public void Triangulate(Vector2[] points, out List<Vector2> vertices, out List<int> indices)
    {
        vertices = new List<Vector2>();
        indices = new List<int>();
        var p = new Polygon();

        for (int i = 0; i < points.Length; i++)
        {
            p.Add(new Vertex(points[i].x, points[i].y));
        }

        //float subDivisions = Mathf.Sqrt(points.Length);

        //for (int i = 0; i < subDivisions; i++)
        //{
        //    var height = (float)Mathf.Lerp(0, 1, i / subDivisions);
        //    p.Add(new Vertex(0, height));
        //    p.Add(new Vertex(2, height));
        //}

        //var contour = new List<Vertex>();

        //contour.Add(new Vertex(0, 0));
        //contour.Add(new Vertex(0, 1));
        //contour.Add(new Vertex(2, 1));
        //contour.Add(new Vertex(2, 0));

        //p.Add(new Contour(contour), false);

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
                        vertices.Add(new Vector2((float)t.GetVertex(j).X, (float)t.GetVertex(j).Y));
                        indices.Add(vertices.Count - 1);
                    }
                }
            }
        }
    }
}
