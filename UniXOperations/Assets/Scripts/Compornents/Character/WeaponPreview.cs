using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPreview : MonoBehaviour
{
    public float RotateSpeed;

    void Update()
    {
        transform.Rotate(new Vector3(0, RotateSpeed, 0) * Time.deltaTime);       
    }
}
