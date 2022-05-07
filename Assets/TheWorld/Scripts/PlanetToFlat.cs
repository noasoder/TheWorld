using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityAtoms.BaseAtoms;

public class PlanetToFlat : FibonacciSphere
{
    [SerializeField]
    private bool render = false;
    [SerializeField]
    private Vector3 origin;
    [SerializeField]
    private FloatReference radius;
    [SerializeField]
    private TextureToSphere planetDepth;
    [SerializeField]
    private ComputeShader shader;

    public ReactiveProperty<List<Vector2>> flat;

    private void Awake()
    {
        samples.ObserveChange().Subscribe(_ => Generate());
    }

    private void Generate()
    {
        flat = new ReactiveProperty<List<Vector2>>();

        var points = GeneratePoints();

        var p = new List<Vector2>();
        foreach (var point in points)
        {
            p.AddRange(PointConversion.SphereToPlane(point).ToArray());
        }

        flat.SetValueAndForceNotify(p);
    }

    private void OnDrawGizmos()
    {
        if(!render)
            return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        for (int i = 0; i < flat.Value.Count; i++)
        {
            Vector3 point = flat.Value[i];
            Gizmos.DrawCube(origin + point * radius.Value, Vector3.one);
        }
    }
}
