using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

public class ArmController : MonoBehaviour
{
    public GameObject Arm;
    public GameObject Target;
    public float RotateSpeed;
    public float TargetAngle;
    public float LookAtSpeed;

    [ReadOnly]
    public bool _isLookAtMode;

    private float _currentArmAngle;

    private void Update()
    {
        if (_isLookAtMode)
        {
            Vector3 direction = Target.transform.position - Arm.transform.position;
            float YZAngle = Vector3.SignedAngle(Arm.transform.forward, direction, Arm.transform.right);
            _currentArmAngle += YZAngle * LookAtSpeed * Time.deltaTime;
            Arm.transform.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
        }
        else
        {
            if (Mathf.Abs(_currentArmAngle - TargetAngle) > 1f)
            {
                float direction = _currentArmAngle < TargetAngle ? 1 : -1;
                float rotateSpeed = RotateSpeed * direction * Time.deltaTime;
                if (rotateSpeed > Mathf.Abs(TargetAngle - _currentArmAngle))
                {
                    rotateSpeed = TargetAngle - _currentArmAngle;
                }
                _currentArmAngle += rotateSpeed;
                Arm.transform.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
            }
        }
    }

    public void LookAtMode()
    {
        _isLookAtMode = true;
    }

    public void TargetAngleMode()
    {
        _isLookAtMode = false;
        _currentArmAngle = Arm.transform.localEulerAngles.x;
        if (_currentArmAngle > 180)
        {
            _currentArmAngle -= 360;
        }
    }

    public void MuzzleJump(float jumpMagnitude)
    {
        Arm.transform.localEulerAngles = new Vector3(_currentArmAngle - jumpMagnitude, 0, 0);
        _currentArmAngle = Arm.transform.localEulerAngles.x;
    }
}
