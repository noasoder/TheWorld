using System.Collections.Generic;
using UnityEngine;
using UniRx;

public abstract class PointSphere : MonoBehaviour
{
    public abstract ReactiveProperty<List<List<Vector3>>> Points { get; set; }
    public abstract int NumMeshes { get; set; }
}
