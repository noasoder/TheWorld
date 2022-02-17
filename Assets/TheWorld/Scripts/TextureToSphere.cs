using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class TextureToSphere : PointSphere
{
    [Header("TextureToSphere")]
    [SerializeField]
    private bool renderDots = true;
    [SerializeField]
    private bool redAsGrey = true;
    public FloatVariable radius;
    public FloatVariable heightMultiplier;
    [SerializeField]
    private FloatReference gizmoSize;

    [Header("Compute")]
    [SerializeField]
    private Texture2D depthMap;
    public ComputeShader shader;

    public ReactiveProperty<Vector4[]> colors;
    public ReactiveProperty<Vector3[]> flatPoints;

    public virtual void Awake()
    {
        base.Awake();
        var points = this.points.Subscribe(points =>
        {
            flatPoints.SetValueAndForceNotify(SphereToFlat(points));
        });
    }

    public Vector4[] ColorToSphere(List<Vector3> points)
    {
        var readBuffer = new ComputeBuffer(points.Count, sizeof(float) * 3);
        var colorBuffer = new ComputeBuffer(points.Count, sizeof(float) * 4);
        readBuffer.SetData(points);

        var id = shader.FindKernel("TextureToSphere");

        shader.SetBuffer(id, "readPoints", readBuffer);
        shader.SetBuffer(id, "colors", colorBuffer);
        shader.SetTexture(id, "ColorMap", depthMap);

        shader.Dispatch(id, points.Count / 16, 1, 1);

        var color = new Vector4[points.Count];
        colorBuffer.GetData(color);
        return color;
    }

    public Vector3[] SphereToFlat(List<Vector3> points)
    {
        var flatPoints = new ComputeBuffer(points.Count, sizeof(float) * 3);
        flatPoints.SetData(points);

        var id = shader.FindKernel("SphereToPlane");

        shader.SetBuffer(id, "flatPoints", flatPoints);

        shader.Dispatch(id, points.Count / 16, 1, 1);

        var flat = new Vector3[points.Count];
        flatPoints.GetData(flat);
        return flat;
    }

    private void OnDrawGizmos()
    {
        if (!renderDots)
            return;

        for (int i = 0; i < colors.Value.Length; i++)
        {
            Vector4 color = colors.Value[i];
            Vector3 pos = this.points.Value[i];
            if(redAsGrey)
                Gizmos.color = new Color(color.x, color.x, color.x, color.w);
            else
                Gizmos.color = new Color(color.x, color.y, color.z, color.w);
            Vector3 scaledPos = new Vector3(pos.x, pos.y, pos.z) * radius.Value;
            Vector3 heightPos = scaledPos + (scaledPos - transform.position).normalized * heightMultiplier.Value * color.x;
            Gizmos.DrawCube(heightPos, Vector3.one * gizmoSize.Value);
        }
    }
}
