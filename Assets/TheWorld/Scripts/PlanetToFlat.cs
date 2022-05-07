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

    public ReactiveProperty<Vector2[]> flat;

    private void Awake()
    {
        samples.ObserveChange().Subscribe(_ => Generate());
    }

    private void Generate()
    {
        flat = new ReactiveProperty<Vector2[]>();

        var points = GeneratePoints();
        
        var spPoints = new ComputeBuffer(points.Count, sizeof(float) * 3);
        var uvs = new ComputeBuffer(points.Count, sizeof(float) * 2);
        spPoints.SetData(points.ToArray());

        var id = shader.FindKernel("SphereToPlane");

        shader.SetBuffer(id, "spPoints", spPoints);
        shader.SetBuffer(id, "uvs", uvs);

        shader.Dispatch(id, points.Count / 16, 1, 1);

        var p = new Vector2[points.Count];
        uvs.GetData(p);

        //Remove not computed
        for (int i = 0; i < p.Length % 16; i++)
        {
            p[p.Length - 1 - i] = new Vector2(1, 0);
        }

        flat.SetValueAndForceNotify(p);
    }

    private void OnDrawGizmos()
    {
        if(!render)
            return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        for (int i = 0; i < flat.Value.Length; i++)
        {
            Vector3 point = flat.Value[i];
            Gizmos.DrawCube(origin + point * radius.Value, Vector3.one);
        }
    }
}
