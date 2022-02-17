using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class PointSphere : MonoBehaviour
{
    [Header("PointSphere")]
    [SerializeField]
    private IntVariable samples;

    public ReactiveProperty<List<Vector3>> points;

    public virtual void Awake()
    {
        points = new ReactiveProperty<List<Vector3>>();

        FibonacciSphere(samples.Value);

        samples.ObserveChange().Subscribe(samples => FibonacciSphere(samples)).AddTo(this);
    }

    private void FibonacciSphere(int samples = 1000)
    {
        var p = new List<Vector3>();
        var phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < samples; i++)
        {
            var y = 1 - (i / (float)(samples - 1)) * 2;
            var radius = Mathf.Sqrt(1 - y * y);

            var theta = phi * i;

            var x = Mathf.Cos(theta) * radius;
            var z = Mathf.Sin(theta) * radius;

            p.Add(new Vector3(x, y, z));
        }
        points.SetValueAndForceNotify(p);
    }
}
