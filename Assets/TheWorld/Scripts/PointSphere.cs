using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityAtoms;
using UniRx;

public class PointSphere : MonoBehaviour
{
    [SerializeField]
    private IntVariable samples;
    [SerializeField]
    private FloatVariable radius;
    [SerializeField]
    private FloatReference gizmoSize;

    private List<Vector3> points = new List<Vector3>();

    private void Awake()
    {
        FibonacciSphere(samples.Value, radius.Value);

        samples.ObserveChange().Subscribe(samples => FibonacciSphere(samples, radius.Value)).AddTo(this);
        radius.ObserveChange().Subscribe(radius => FibonacciSphere(samples.Value, radius)).AddTo(this);
    }

    private void FibonacciSphere(int samples = 1000, float size = 10)
    {
        points.Clear();
        var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < samples; i++)
        {
            var y = 1 - (i / (float)(samples - 1)) * 2;
            var radius = Mathf.Sqrt(1 - y * y);

            var theta = phi * i;

            var x = Mathf.Cos(theta) * radius;
            var z = Mathf.Sin(theta) * radius;

            points.Add(new Vector3(x, y, z) * size);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var point in points)
        {
            Gizmos.DrawCube(point, Vector3.one * gizmoSize.Value);
        }
    }
}
