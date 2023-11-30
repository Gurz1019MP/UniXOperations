using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPreviewCamera : MonoBehaviour
{
    public GameObject Target;

    void Start()
    {
        transform.LookAt(Target.transform);
    }
}
