using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityAtoms.BaseAtoms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

public class PlanetMesh : TextureToSphere
{
    [Header("PlanetMesh")]
    [SerializeField]
    private MeshFilter flatPlanet;

    [SerializeField]
    private string planetName;
    [SerializeField]
    private Material planetMaterial;

    private List<MeshFilter> models;
    private List<MeshRenderer> renderers;

    private string savePath;
    private List<List<Vector2>> points;

    public async void Generate()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        savePath = "Assets/TheWorld/GeneratedPlanets/" + planetName;
        points = PointToFlat(pointSphere.GeneratePoints());
        models = new List<MeshFilter>(transform.GetComponentsInChildren<MeshFilter>());
        renderers = new List<MeshRenderer>(transform.GetComponentsInChildren<MeshRenderer>());

        List<Task> t = new List<Task>();
        for (int i = 0; i < 6; i++)
        {
            t.Add(GenerateMesh(i));
        }
        await Task.WhenAll(t);

        stopwatch.Stop();
        var elapsedTime = stopwatch.Elapsed;
        UnityEngine.Debug.Log($"Generated planet in {elapsedTime} seconds");
    }

    public async Task GenerateMesh(int i)
    {
        List<Vector3> p = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> verts = new List<Vector2>();
        List<Vector2> uvs = new List<Vector2>();

        if (models.Count <= i)
        {
            var newModel = new GameObject("Model " + i);
            newModel.transform.SetParent(transform, false);
            models.Add(newModel.AddComponent<MeshFilter>());

            var meshRenderer = newModel.AddComponent<MeshRenderer>();
            meshRenderer.material = planetMaterial;
            renderers.Add(meshRenderer);
        }

        await Task.Run(() =>
        {
            Triangulate(points[0].ToArray(), out verts, out indices);
            p = PointConversion.PlaneToSphere(verts);
            uvs = PointConversion.SphereToUV(p);

            if (i <= 3)
            {
                //Rotate
                var rotation = new Vector3(0, i <= 3 ? i * 90 : 0, 0);
                for (int j = 0; j < p.Count; j++)
                {
                    p[j] = (Quaternion.Euler(rotation) * p[j]);
                }
            }
            else
            {
                //Rotate
                var rotation = new Vector3(i == 4 ? 90 : i == 5 ? -90 : 0, 0, 0);
                for (int j = 0; j < p.Count; j++)
                {
                    p[j] = (Quaternion.Euler(rotation) * p[j]);
                }
            }

        });

        var colors = ColorToSphere(new List<Vector3>(p));
        models[i].sharedMesh = new Mesh();

        for (int j = 0; j < p.Count; j++)
        {
            p[j] *= radius.Value + heightMultiplier.Value * colors[j].x;
        }
        models[i].sharedMesh.Clear();
        models[i].sharedMesh.vertices = p.ToArray();
        models[i].sharedMesh.triangles = indices.ToArray();
        models[i].sharedMesh.SetUVs(0, uvs);
        models[i].sharedMesh.Optimize();
        models[i].sharedMesh.RecalculateBounds();
        models[i].sharedMesh.RecalculateTangents();
        models[i].sharedMesh.RecalculateNormals();

        var material = new Material(planetMaterial);

        if(i <= 3)
        {
            material.SetFloat("_AngleX", i * 90);
        }
        else
        {
            material.SetFloat("_AngleY", i == 4 ? 90 : 270);
        }
        renderers[i].sharedMaterial = material;

        var modelPath = savePath + i + ".asset";
        AssetDatabase.DeleteAsset(modelPath);
        AssetDatabase.CreateAsset(models[i].sharedMesh, modelPath);

        //UnityEngine.Debug.Log($"Done {i}");
        if (!flatPlanet)
        {
            return;
        }

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
