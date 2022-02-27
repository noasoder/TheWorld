using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private Vector3 rotateAxis;

    void Update()
    {
        transform.rotation *= Quaternion.Euler(rotateAxis.normalized * rotateSpeed);        
    }
}
