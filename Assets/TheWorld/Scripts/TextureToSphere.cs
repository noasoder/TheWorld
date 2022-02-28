using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class TextureToSphere : MonoBehaviour
{
    [SerializeField]
    private PointSphere pointSphere;

    [Header("TextureToSphere")]
    [SerializeField]
    private bool renderDots = true;
    [SerializeField]
    private bool redAsGrey = true;
    [SerializeField]
    public FloatReference radius;
    [SerializeField]
    public FloatReference heightMultiplier;
    [SerializeField]
    private FloatReference gizmoSize;

    [Header("Compute")]
    [SerializeField]
    private Texture2D depthMap;
    public ComputeShader shader;

    public ReactiveProperty<List<List<Vector4>>> colors;
    public ReactiveProperty<List<List<Vector2>>> flatPoints;

    public virtual void Start()
    {
        this.pointSphere.Points.Subscribe(points =>
        {
            flatPoints.SetValueAndForceNotify(GetUVs(points));
        }).AddTo(this);
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

        shader.Dispatch(id, points.Count, 1, 1);

        var color = new Vector4[points.Count];
        colorBuffer.GetData(color);
        return color;
    }

    public List<List<Vector2>> GetUVs(List<List<Vector3>> pointGroups, float xOffset = 0, float yOffset = 0)
    {
        var result = new List<List<Vector2>>();
        foreach (var points in pointGroups)
        {
            var spPoints = new ComputeBuffer(points.Count, sizeof(float) * 3);
            var uvs = new ComputeBuffer(points.Count, sizeof(float) * 2);
            spPoints.SetData(points);

            var id = shader.FindKernel("SphereToPlane");

            shader.SetBuffer(id, "spPoints", spPoints);
            shader.SetBuffer(id, "uvs", uvs);
            shader.SetFloat("xOffset", xOffset);
            shader.SetFloat("yOffset", yOffset);

            shader.Dispatch(id, points.Count, 1, 1);

            var p = new Vector2[points.Count];
            uvs.GetData(p);

            spPoints.Release();
            uvs.Release();

            //Remove not computed
            //for (int i = 0; i < p.Length % 16; i++)
            //{
            //    p[p.Length - 1 - i] = new Vector2(-1, -1);
            //}
            result.Add(new List<Vector2>(p));
        }
        return result;
    }

    private void OnDrawGizmos()
    {
        if (!renderDots)
            return;

        for (int i = 0; i < colors.Value.Count; i++)
        {
            for (int j = 0; j < colors.Value[i].Count; j++)
            {
                Vector4 color = colors.Value[i][j];
                Vector3 pos = this.pointSphere.Points.Value[i][j];
                if(redAsGrey)
                    Gizmos.color = new Color(color.x, color.x, color.x, color.w);
                else
                    Gizmos.color = new Color(color.x, color.y, color.z, color.w);
                Vector3 scaledPos = new Vector3(pos.x, pos.y, pos.z) * radius.Value;
                Vector3 heightPos = scaledPos + (scaledPos).normalized * heightMultiplier.Value * color.x;
                Gizmos.DrawCube(heightPos, Vector3.one * gizmoSize.Value);
            }
        }
    }
}
