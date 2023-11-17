using UnityEngine;

public class ArmController : MonoBehaviour
{
    public GameObject Arm;
    public GameObject Target;
    public float RotateSpeed;
    public float TargetAngle;

    [ReadOnly]
    public bool _isLookAtMode;

    private float _currentArmAngle;

    private void Update()
    {
        if (_isLookAtMode)
        {
            Arm.transform.LookAt(Target.transform);
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
                _currentArmAngle = _currentArmAngle + rotateSpeed;
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
}
