using UniRx;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using System.Collections.Generic;

public class CubeSphere : PointSphere
{
    [Header("PointSphere")]
    [SerializeField]
    private IntReference subdivisions;

    public override List<List<Vector3>> GeneratePoints()
    {
        var c = CreateCube(subdivisions.Value);
        return CreateCubeSphereNorm(c);
    }

    private List<List<Vector3>> CreateCube(int subs = 16)
    {
        List<List<Vector3>> p = new List<List<Vector3>>();
        p.Add(CreateCubeSection(subs));
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
}
