using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityAtoms.BaseAtoms;

public class PlanetToFlat : MonoBehaviour
{
    [SerializeField]
    private bool render = false;
    [SerializeField]
    private Vector3 origin;
    [SerializeField]
    private FloatVariable radius;
    [SerializeField]
    private TextureToSphere planetDepth;
    [SerializeField]
    private PointSphere sphere;
    [SerializeField]
    private ComputeShader shader;

    public ReactiveProperty<Vector3[]> flat;

    private void Awake()
    {
        flat = new ReactiveProperty<Vector3[]>();

        sphere.points.Select(points =>
        {
            var spPoints = new ComputeBuffer(points.Count, sizeof(float) * 3);
            spPoints.SetData(points);

            var id = shader.FindKernel("SphereToPlane");

            shader.SetBuffer(id, "spPoints", spPoints);

            shader.Dispatch(id, points.Count / 16, 1, 1);

            var p = new Vector3[points.Count];
            spPoints.GetData(p);

            //Remove not computed
            for (int i = 0; i < p.Length % 16; i++)
            {
                p[p.Length - 1 - i] = new Vector3(1, 0, 0);
            }

            flat.SetValueAndForceNotify(p);
            return p;
        }).ToReactiveProperty();
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
