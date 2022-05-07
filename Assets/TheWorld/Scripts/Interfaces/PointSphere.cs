using System.Collections.Generic;
using UnityEngine;
using UniRx;

public abstract class PointSphere : MonoBehaviour
{
    public abstract List<List<Vector3>> GeneratePoints();
}
