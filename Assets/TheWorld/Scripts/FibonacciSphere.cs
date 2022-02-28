using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class FibonacciSphere : PointSphere
{
    [Header("PointSphere")]
    [SerializeField]
    private IntVariable samples;

    [SerializeField]
    private bool renderDots = true;
    [SerializeField]
    private FloatReference gizmoSize;
    [SerializeField]
    private FloatReference radius;

    public override ReactiveProperty<List<List<Vector3>>> Points { get; set; }
    public override int NumMeshes { get; set; }

    public void Awake()
    {
        NumMeshes = 1;
        Points = new ReactiveProperty<List<List<Vector3>>>();

        CreateFibonacciSphere(samples.Value);

        samples.ObserveChange().Subscribe(samples => CreateFibonacciSphere(samples)).AddTo(this);
    }

    private void CreateFibonacciSphere(int samples = 1000)
    {
        var p = new List<List<Vector3>>();
        var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        p.Add(new List<Vector3>());
        for (int i = 0; i < samples; i++)
        {
            var y = 1 - (i / (float)(samples - 1)) * 2;
            var radius = Mathf.Sqrt(1 - y * y);

            var theta = phi * i;

            var x = Mathf.Cos(theta) * radius;
            var z = Mathf.Sin(theta) * radius;

            p[0].Add(new Vector3(x, y, z));
        }
        Points.SetValueAndForceNotify(p);
    }

    private void OnDrawGizmos()
    {
        if (!renderDots || Points == null)
            return;

        for (int i = 0; i < Points.Value.Count; i++)
        {
            for (int j = 0; j < Points.Value[i].Count; j++)
            {
                Gizmos.DrawCube(Points.Value[i][j] * radius.Value, Vector3.one * gizmoSize.Value);
            }
        }
    }
}
