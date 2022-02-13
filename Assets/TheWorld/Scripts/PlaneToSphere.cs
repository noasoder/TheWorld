using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UniRx;

public class PlaneToSphere : MonoBehaviour
{
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

            var id = shader.FindKernel("PlaneToSphere");

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
        for (int i = 0; i < depthPoints.Length; i++)
        {
            Vector4 point = depthPoints[i];
            Vector3 pos = pointSphere.points.Value[i];
            if(redAsGrey)
                Gizmos.color = new Color(point.x, point.x, point.x, point.w);
            else
                Gizmos.color = new Color(point.x, point.y, point.z, point.w);
            Vector3 scaledPos = new Vector3(pos.x, pos.y, pos.z) * radius.Value + Vector3.one * heightMultiplier.Value * point.x;
            Gizmos.DrawCube(scaledPos, Vector3.one * gizmoSize.Value);
        }
    }
}
