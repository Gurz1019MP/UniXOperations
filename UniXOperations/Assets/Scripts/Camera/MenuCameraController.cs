using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    public Vector3 LocalPosition;

    [ReadOnly]
    public Transform target;

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + LocalPosition;
            transform.LookAt(target);
        }
    }

    public void Initialize(Character characterState)
    {
        target = characterState.FpsCameraAnchor;
    }
}
