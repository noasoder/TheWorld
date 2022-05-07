using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class FibonacciSphere : PointSphere
{
    [Header("PointSphere")]
    [SerializeField]
    protected IntVariable samples;

    public override List<List<Vector3>> GeneratePoints()
    {
        var p = new List<List<Vector3>>();
        var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        p.Add(new List<Vector3>());
        for (int i = 0; i < samples.Value; i++)
        {
            var y = 1 - (i / (float)(samples.Value - 1)) * 2;
            var radius = Mathf.Sqrt(1 - y * y);

            var theta = phi * i;

            var x = Mathf.Cos(theta) * radius;
            var z = Mathf.Sin(theta) * radius;

            p[0].Add(new Vector3(x, y, z));
        }

        return p;
    }
}
