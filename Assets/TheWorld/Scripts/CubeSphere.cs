using UniRx;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using System.Collections.Generic;

public class CubeSphere : PointSphere
{
    [Header("PointSphere")]
    [SerializeField]
    private IntVariable subdivisions;

    [SerializeField]
    private bool renderDots = true;
    [SerializeField]
    private FloatReference gizmoSize;
    [SerializeField]
    private FloatReference radius;

    public override ReactiveProperty<List<List<Vector3>>> Points { get; set; }
    public override int NumMeshes { get; set; }

    private List<List<Vector3>> cube = new List<List<Vector3>>();

    public void Awake()
    {
        NumMeshes = 4;
        Points = new ReactiveProperty<List<List<Vector3>>>();

        Generate(subdivisions.Value);

        subdivisions.ObserveChange().Subscribe(subdivisions => Generate(subdivisions)).AddTo(this);
    }

    private void Generate(int subdivisions)
    {
        var c = CreateCube(subdivisions);
        var sphere = CreateCubeSphereNorm(c);
        cube = c;
        Points.SetValueAndForceNotify(sphere);
    }

    private List<List<Vector3>> CreateCube(int subs = 16)
    {
        List<List<Vector3>> p = new List<List<Vector3>>();
        p.Add(CreateCubeSection(subs));

        //for (int i = 1; i < 6; i++)
        //{
        //    p.Add(new List<Vector3>());
        //    var rotation = new Vector3(i == 4 ? 90 : i == 5 ? -90 : 0, i <= 3 ? i * 90 : 0, 0);
        //    Debug.Log(rotation);
        //    for (int j = 0; j < p[0].Count; j++)
        //    {
        //        p[i].Add(Quaternion.Euler(rotation) * p[0][j]);
        //    }
        //}

        return p;
    }

    private List<Vector3> CreateCubeSection(int subs = 16)
    {
        var p = new List<Vector3>();

        const float offset = 0.5f;

        //Corners
        p.Add(new Vector3(-offset, offset, -offset));
        p.Add(new Vector3(offset, offset, -offset));
        p.Add(new Vector3(-offset, -offset, -offset));
        p.Add(new Vector3(offset, -offset, -offset));

        //Edges
        for (int x = 1; x < subs; x++)
        {
            p.Add(new Vector3(Mathf.Lerp(0, 1, x / (float)subs) - offset, -offset, -offset));
            p.Add(new Vector3(Mathf.Lerp(0, 1, x / (float)subs) - offset, offset, -offset));
            p.Add(new Vector3(-offset, Mathf.Lerp(0, 1, x / (float)subs) - offset, -offset));
            p.Add(new Vector3(offset, Mathf.Lerp(0, 1, x / (float)subs) - offset, -offset));
        }

        for (int x = 1; x < subs; x++)
        {
            for (int y = 1; y < subs; y++)
            {
                p.Add(new Vector3(Mathf.Lerp(0, 1, x / (float)subs) - offset, Mathf.Lerp(0, 1, y / (float)subs) - offset, -offset));
            }
        }

        return p;
    }

    private List<Vector3> CreateCubeSphere(List<Vector3> cube)
    {
        var p = new List<Vector3>();
        foreach (var point in cube)
        {
            p.Add(point.normalized);
        }
        return p;
    }

    private List<List<Vector3>> CreateCubeSphereNorm(List<List<Vector3>> cube)
    {
        for (int i = 0; i < cube.Count; i++)
        {
            for (int j = 0; j < cube[i].Count; j++)
            {
                var x = cube[i][j].x * Mathf.Sqrt(1 - (cube[i][j].y * cube[i][j].y * 0.5f) - (cube[i][j].z * cube[i][j].z * 0.5f) + (cube[i][j].y * cube[i][j].y * cube[i][j].z * cube[i][j].z) / 3);
                var y = cube[i][j].y * Mathf.Sqrt(1 - (cube[i][j].z * cube[i][j].z * 0.5f) - (cube[i][j].x * cube[i][j].x * 0.5f) + (cube[i][j].z * cube[i][j].z * cube[i][j].x * cube[i][j].x) / 3);
                var z = cube[i][j].z * Mathf.Sqrt(1 - (cube[i][j].x * cube[i][j].x * 0.5f) - (cube[i][j].y * cube[i][j].y * 0.5f) + (cube[i][j].x * cube[i][j].x * cube[i][j].y * cube[i][j].y) / 3);
                cube[i][j] = new Vector3(x, y, z).normalized;
            }
        }

        return cube;
    }

    private void OnDrawGizmos()
    {
        if (!renderDots || Points == null || Points.Value == null)
            return;

        Color[] color = { Color.blue, Color.cyan, Color.green, Color.gray, Color.red, Color.yellow };

        for (int i = 0; i < Points.Value.Count; i++)
        {
            Gizmos.color = color[i];
            for (int j = 0; j < Points.Value[0].Count; j++)
            {
                Gizmos.DrawCube(Points.Value[i][j] * radius.Value, Vector3.one * gizmoSize.Value);
            }
        }
        for (int i = 0; i < cube.Count; i++)
        {
            Gizmos.color = color[i];
            for (int j = 0; j < cube[0].Count; j++)
            {
                Gizmos.DrawCube(cube[i][j] * radius.Value, Vector3.one * gizmoSize.Value * 0.5f);
            }
        }
    }
}
