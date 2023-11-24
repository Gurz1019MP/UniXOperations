using UnityEngine;
using UniRx;

/// <summary>
/// FPS的な移動を表現する
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterState))]
[RequireComponent(typeof(CharacterInputterContainer))]
public class FPSMover : MonoBehaviour
{
    public float _walkSpeed;        // 一般的な成人の歩行スピード
    public float _runMultiplier;    // 走ったときの速度係数
    public float _gravity;          // 重力
    public float _jumpPower;        // ジャンプ力
    public float _slidingForce;     // スライディング力
    public float _groundThreshold = 0.01f;

    private CharacterController _characterController;
    private Vector3 _moveDelta;
    private Animator _characterAnimator;
    private CharacterState _characterState;
    private CharacterInputterContainer _characterInputterContainer;
    private float _animatorFactor = 0.0f;

    public bool IsGround { get; set; }

    public ICharacterInputter Inputter => _characterInputterContainer.Inputter;

    public void Initialize(CharacterState characterState)
    {
        _characterState = characterState;
        _characterController = characterState.CharacterController;
        _characterInputterContainer = _characterState.InputterContainer;
        _characterAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        _moveDelta.x = 0;
        _moveDelta.z = 0;

        Vector3 rayPoint = transform.position + _characterController.center + Vector3.up * (-_characterController.height * 0.5F + _characterController.radius);
        bool newIsGround = GetIsGround(rayPoint);
        RaycastHit[] slideHits = Physics.RaycastAll(rayPoint, Vector3.down, float.MaxValue, LayerMask.GetMask("Stage"));
        float tilt = slideHits.Length == 1 && newIsGround ? Mathf.Acos(Vector3.Dot(slideHits[0].normal, Vector3.up)) : 0;
        bool isSlide = tilt > _characterController.slopeLimit / 180F * Mathf.PI;

        //if (_characterState.ID == 0)
        //{
        //    Debug.Log(hits.Length > 0 ? $"IsGround : {newIsGround}, hit count : {hits.Length}, distance : {Mathf.Abs(hits[0].distance - _characterController.skinWidth)}, tilt : {tilt / Mathf.PI * 180}" : "なし");
        //}

        // 着地判定
        if (!IsGround && newIsGround)
        {
            // 落下ダメージ
            if (_moveDelta.y < -8)
            {
                _characterState.TakeDamage((_moveDelta.y + 7) * -50, Vector3.zero);
            }

            _moveDelta.y = 0;
        }

        // 設置フラグを更新
        IsGround = newIsGround;

        // 地に足ついている時の処理
        if (IsGround)
        {
            if (!isSlide && Inputter.JumpEnter)
            {
                _moveDelta.y = _jumpPower;
                IsGround = false;
            }
            else
            {
                _moveDelta.y = -_gravity * Time.deltaTime;
            }
        }
        else
        {
            _moveDelta.y -= _gravity * Time.deltaTime;
        }

        // 入力処理
        if (Inputter != null)
        {
            if (Inputter.Walk)
            {
                var inputDelta = Vector3.forward * _walkSpeed;
                inputDelta = transform.TransformDirection(inputDelta);

                _moveDelta.x += inputDelta.x;
                _moveDelta.z += inputDelta.z;

                _characterAnimator.SetBool(_animatorParamIsWalk, true);
                _characterAnimator.SetBool(_animatorParamIsRun, false);
                _animatorFactor = 1.0f;
            }
            else
            {
                var inputDelta = new Vector3(Inputter.Horizontal, 0, Inputter.Vertical).normalized * _walkSpeed;

                if (inputDelta.z == 0)
                {
                    _characterAnimator.SetBool(_animatorParamIsWalk, false);
                    _characterAnimator.SetBool(_animatorParamIsRun, false);
                    _animatorFactor = 0;
                }
                else if (inputDelta.z > 0)
                {
                    inputDelta.z *= _runMultiplier;

                    _characterAnimator.SetBool(_animatorParamIsWalk, false);
                    _characterAnimator.SetBool(_animatorParamIsRun, true);
                    _animatorFactor = 1.0f;
                }
                else
                {
                    _characterAnimator.SetBool(_animatorParamIsWalk, true);
                    _characterAnimator.SetBool(_animatorParamIsRun, false);
                    _animatorFactor = -1.0f;
                }


                inputDelta = transform.TransformDirection(inputDelta);

                _moveDelta.x += inputDelta.x;
                _moveDelta.z += inputDelta.z;
            }

            _characterAnimator.SetFloat(_animatorParamFactor, _animatorFactor);
        }

        // 斜面のスライド
        if (isSlide && IsGround)
        {
            Vector3 reactionForce = slideHits[0].normal * _slidingForce;

            _moveDelta.x += reactionForce.x;
            _moveDelta.y -= reactionForce.y;
            _moveDelta.z += reactionForce.z;
        }

        _characterController.Move(_moveDelta * Time.deltaTime);
    }

    private bool GetIsGround(Vector3 rayPoint)
    {
        if (_characterController.isGrounded)
        {
            return true;
        }
        else
        {
            RaycastHit[] hits = Physics.SphereCastAll(rayPoint, _characterController.radius, Vector3.down, float.MaxValue, LayerMask.GetMask("Stage"));
            if (hits.Length == 1)
            {
                return Mathf.Abs(hits[0].distance - _characterController.skinWidth) < _groundThreshold;
            }
            else
            {
                return false;
            }
        }

    }

    private void DrawDebugCross(Vector3 point, float scale, Color color, float duration)
    {
        Debug.DrawLine(point + Vector3.up * scale, point + Vector3.up * -scale, color, duration);
        Debug.DrawLine(point + Vector3.right * scale, point + Vector3.right * -scale, color, duration);
        Debug.DrawLine(point + Vector3.forward * scale, point + Vector3.forward * -scale, color, duration);
    }

    private static readonly string _animatorParamIsWalk = "IsWalk";
    private static readonly string _animatorParamIsRun = "IsRun";
    private static readonly string _animatorParamFactor = "Factor";
}
