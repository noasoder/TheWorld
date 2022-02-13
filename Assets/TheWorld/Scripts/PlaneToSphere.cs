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
    private FloatReference gizmoSize;

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
        foreach (var point in depthPoints)
        {
            Gizmos.color = new Color(point.w, point.w, point.w, 1);
            Gizmos.DrawCube(new Vector3(point.x, point.y, point.z) * radius.Value, Vector3.one * gizmoSize.Value);
        }
    }
}
