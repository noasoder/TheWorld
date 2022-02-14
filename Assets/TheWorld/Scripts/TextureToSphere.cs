using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class TextureToSphere : MonoBehaviour
{
    [SerializeField]
    private bool render = true;
    [SerializeField]
    private FloatVariable radius;
    [SerializeField]
    private FloatVariable heightMultiplier;
    [SerializeField]
    private FloatReference gizmoSize;
    [SerializeField]
    private bool redAsGrey = true;

    [Header("Compute")]
    [SerializeField]
    private Texture2D depthMap;
    [SerializeField]
    private ComputeShader shader;
    [SerializeField]
    private PointSphere pointSphere;

    public Vector4[] depthPoints;

    private void Start()
    {
        var points = pointSphere.points.Subscribe(points =>
        {
            var readBuffer = new ComputeBuffer(points.Count, sizeof(float) * 3);
            var writeBuffer = new ComputeBuffer(points.Count, sizeof(float) * 4);
            readBuffer.SetData(points);

            var id = shader.FindKernel("TextureToSphere");

            shader.SetBuffer(id, "readPoints", readBuffer);
            shader.SetBuffer(id, "writePoints", writeBuffer);
            shader.SetTexture(id, "DepthMap", depthMap);

            shader.Dispatch(id, points.Count / 16, 1, 1);

            depthPoints = new Vector4[points.Count];
            writeBuffer.GetData(depthPoints);
        });
    }

    private void OnDrawGizmos()
    {
        if (!render)
            return;

        for (int i = 0; i < depthPoints.Length; i++)
        {
            Vector4 color = depthPoints[i];
            Vector3 pos = pointSphere.points.Value[i];
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
