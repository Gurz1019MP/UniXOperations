using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiedCameraController : MonoBehaviour
{
    public float Speed;

    [ReadOnly]
    public Vector3 TargetPoint;

    void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.up * -1, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask(ConstantsManager.LayerMask_Stage)))
        {
            TargetPoint = hitInfo.point;
        }
    }

    void Update()
    {
        Vector3 direction = TargetPoint - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction, transform.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Speed * Time.deltaTime);

        transform.Rotate(new Vector3(0, 0, -0.2f));
    }
}
