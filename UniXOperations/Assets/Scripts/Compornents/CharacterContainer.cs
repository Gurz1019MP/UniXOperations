using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(CharacterController))]
public class CharacterContainer : MonoBehaviour
{
    #region Field

    public TextMesh DebugText;

    #endregion

    #region プロパティ

    public Character Character { get; private set; }

    public CharacterController CharacterController { get; private set; }

    public Transform FpsCameraAnchor { get; private set; }

    public Transform TpsCameraAnchor { get; private set; }

    public Transform Target { get; private set; }

    public Transform UpBase { get; private set; }

    public Transform LegBase { get; private set; }

    public Transform ArmBase { get; private set; }

    public Transform CurrentHand { get; private set; }

    #endregion

    #region イベント

    public System.IObservable<Unit> OnWeaponVisualUpdated => _onWeaponVisualUpdatedSubject;

    #endregion

    public void InitCharacterContainer(Character character)
    {
        Character = character;

        CharacterController = GetComponent<CharacterController>();
        _characterAnimator = GetComponentInChildren<Animator>();

        InitHierarchyObject();

        UpdateWeaponVisual();

        // モデルの読み込み
        try
        {
            _visual = new VarialbeVisual(Character.Spec);

            // メッシュの読み込み
            _visual.ChangeMesh(UpBase.gameObject);

            // マテリアルの読み込み
            foreach (var target in _arms.Concat(new GameObject[] { UpBase.gameObject, LegBase.gameObject }))
            {
                _visual.ChangeMaterial(target);
            }
        }
        catch
        {
            throw new System.InvalidOperationException("CharacterContainerのビジュアルの初期化で例外が発生しました");
        }

        _currentXAngle = FpsCameraAnchor.transform.localEulerAngles.x;

        Character.OnDied.Subscribe((_) => Kill()).AddTo(gameObject);
        Character.OnFire.Subscribe(Character.CombatStatistics.OnFire).AddTo(gameObject);
        Character.OnHit.Subscribe(Character.CombatStatistics.OnHit).AddTo(gameObject);
        Character.OnWeaponVisualUpdated.Subscribe((_) => UpdateWeaponVisual()).AddTo(gameObject);
        Character.OnGrenadeFire.Subscribe(GrenadeFire).AddTo(gameObject);
        Character.OnGunFire.Subscribe(GunFire).AddTo(gameObject);
        Character.OnZombieFire.Subscribe(ZombieFire).AddTo(gameObject);
        Character.OnCameraMuzzleJump.Subscribe(CameraMuzzleJump).AddTo(gameObject);
        Character.OnDropWeapon.Subscribe((_) => DropWeapon(Character.State.CurrentWeapon)).AddTo(gameObject);
    }

    private void InitHierarchyObject()
    {
        List<GameObject> arms = new List<GameObject>();
        foreach (var transform in GetComponentsInChildren<Transform>(true))
        {
            if (transform.CompareTag(ConstantsManager.TagFpsCameraAnchor))
            {
                FpsCameraAnchor = transform;
            }
            else if (transform.CompareTag(ConstantsManager.TagTpsCameraAnchor))
            {
                TpsCameraAnchor = transform;
            }
            else if (transform.CompareTag(ConstantsManager.TagTarget))
            {
                Target = transform;
            }
            else if (transform.CompareTag(ConstantsManager.TagBody))
            {
                if (transform.name.Equals("Up"))
                {
                    UpBase = transform;
                }
                else if (transform.name.Equals("Leg"))
                {
                    LegBase = transform;
                }
            }
            else if (transform.CompareTag(ConstantsManager.TagArm))
            {
                ArmBase = transform;
            }
            else if (transform.CompareTag(ConstantsManager.TagArmHolding))
            {
                arms.Add(transform.gameObject);
            }
        }
        _arms = arms.ToArray();
    }

    void Update()
    {
        Vector3 rayPoint = transform.position + CharacterController.center + Vector3.up * (-CharacterController.height * 0.5F + CharacterController.radius);
        bool newIsGround = GetIsGround(rayPoint);

        Character.Update(transform.position, newIsGround);

        UpdateMove(rayPoint, newIsGround);

        UpdateLook();

        UpdateArm();
    }

    private void UpdateMove(Vector3 rayPoint, bool newIsGround)
    {
        RaycastHit[] slideHits = Physics.RaycastAll(rayPoint, Vector3.down, float.MaxValue, LayerMask.GetMask(ConstantsManager.LayerMask_Stage));
        float tilt = slideHits.Length == 1 && newIsGround ? Mathf.Acos(Vector3.Dot(slideHits[0].normal, Vector3.up)) : 0;
        bool isSlide = tilt > CharacterController.slopeLimit / 180F * Mathf.PI;

        Vector3 moveDelta = Character.State.MoveDelta;

        moveDelta.x = 0;
        moveDelta.z = 0;

        // 着地判定
        if (!Character.State.IsGround && newIsGround)
        {
            // 落下ダメージ
            if (moveDelta.y < -8)
            {
                Character.TakeDamage((moveDelta.y + 7) * -50, Vector3.zero);
            }

            moveDelta.y = 0;
        }

        // 地に足ついている時の処理
        if (newIsGround)
        {
            if (!isSlide && Character.InputterState.Inputter.JumpEnter)
            {
                moveDelta.y = Character.JUMPPOWER;
                Character.SetState(Character.State.SetIsGround(false));
            }
            else
            {
                moveDelta.y = -Character.GRAVITY * Time.deltaTime;
            }
        }
        else
        {
            moveDelta.y -= Character.GRAVITY * Time.deltaTime;
        }

        if (Character.InputterState.Inputter.Walk)
        {
            var inputDelta = Vector3.forward * Character.WALKSPEED;
            inputDelta = transform.TransformDirection(inputDelta);

            moveDelta.x += inputDelta.x;
            moveDelta.z += inputDelta.z;

            ChangeAnimation(true, false, 1.0f);
        }
        else
        {
            var inputDelta = new Vector3(Character.InputterState.Inputter.Horizontal, 0, Character.InputterState.Inputter.Vertical).normalized * Character.WALKSPEED;

            if (inputDelta.z == 0)
            {
                ChangeAnimation(false, false, 0f);
            }
            else if (inputDelta.z > 0)
            {
                inputDelta.z *= Character.RUMMULTIPLIER;

                ChangeAnimation(false, true, 1.0f);
            }
            else
            {
                ChangeAnimation(true, false, -1.0f);
            }

            inputDelta = transform.TransformDirection(inputDelta);

            moveDelta.x += inputDelta.x;
            moveDelta.z += inputDelta.z;
        }

        // 斜面のスライド
        if (isSlide && newIsGround)
        {
            Vector3 reactionForce = slideHits[0].normal * Character.SLIDINGFORCE;

            moveDelta.x += reactionForce.x;
            moveDelta.y -= reactionForce.y;
            moveDelta.z += reactionForce.z;
        }

        CharacterController.Move(moveDelta * Time.deltaTime);
        Character.SetState(Character.State.SetMoveDelta(moveDelta));
    }

    private void UpdateLook()
    {
        Vector3 rotate = _mouseSensitivity * Time.deltaTime * new Vector3(Character.InputterState.Inputter.MouseY, Character.InputterState.Inputter.MouseX, 0);
        gameObject.transform.Rotate(0, rotate.y, 0);

        _currentXAngle = Mathf.Clamp(_currentXAngle + rotate.x, -_maxAngle, _maxAngle);
        FpsCameraAnchor.transform.localEulerAngles = new Vector3(_currentXAngle, 0, 0);
    }

    private void UpdateArm()
    {
        if (Character.State.IsArmLookAtMode)
        {
            Vector3 direction = Target.transform.position - ArmBase.position;
            float YZAngle = Vector3.SignedAngle(ArmBase.forward, direction, ArmBase.right);
            _currentArmAngle += YZAngle * Character.ARMLOOKATSPEED * Time.deltaTime;
            ArmBase.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
        }
        else
        {
            if (Mathf.Abs(_currentArmAngle - Character.State.ArmTargetAngle) > 1f)
            {
                float direction = _currentArmAngle < Character.State.ArmTargetAngle ? 1 : -1;
                float rotateSpeed = Character.ARMROTATIONSPEED * direction * Time.deltaTime;
                if (rotateSpeed > Mathf.Abs(Character.State.ArmTargetAngle - _currentArmAngle))
                {
                    rotateSpeed = Character.State.ArmTargetAngle - _currentArmAngle;
                }
                _currentArmAngle += rotateSpeed;
                ArmBase.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
            }
        }
    }

    private void Kill()
    {
        DropWeapon(Character.State.Weapon1, true);
        DropWeapon(Character.State.Weapon2, true);

        var instance = Instantiate(CharacterPrefabProvider.Instance.PrefabDiedCharacter, transform.position + Vector3.up * -0.558f, transform.rotation);
        var diedCharacter = instance.GetComponent<DiedCharacter>();
        diedCharacter.Initialize(_visual, false);

        Destroy(gameObject);
    }

    /// <summary>
    /// 武器を拾う
    /// </summary>
    /// <param name="pickedWeapon">拾った武器</param>
    public bool PickupWeapon(WeaponState pickedWeapon)
    {
        if (Character.State.CurrentWeapon.Spec.IsNone && !Character.IsZombie)
        {
            Character.SetState(Character.State.SetCurrentWeaponState(pickedWeapon));
            UpdateWeaponVisual();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 武器を捨てる
    /// </summary>
    private void DropWeapon(WeaponState weapon, bool isRandom = false)
    {
        if (Character.State.IsZoom) Character.ChangeZoom();

        // ピックアップの生成位置を調整
        Vector3 instantinateVector = transform.forward;
        float InstantiateDistance = 0.5f;
        if (isRandom)
        {
            instantinateVector = Quaternion.Euler(0, Random.Range(-60f, 60f), 0) * instantinateVector;
            InstantiateDistance *= Random.Range(1f, 2f);
        }
        RaycastHit[] hits = Physics.RaycastAll(transform.position, instantinateVector, InstantiateDistance, 1 << 16);
        if (hits.Any())
        {
            InstantiateDistance = hits.Min(h => h.distance);
        }
        Vector3 InstantiatePosition = transform.position + instantinateVector * InstantiateDistance;
        //Debug.Log($"InstantiateDistance : {InstantiateDistance}");

        // ピックアップを生成
        var instance = Instantiate(MissionPrefabProvider.Instance.PrefabWeaponPickup, InstantiatePosition, transform.rotation * Quaternion.Euler(0, 0, -90));

        // ピックアップを初期化
        var pickup = instance.GetComponent<WeaponPickup>();
        pickup.Initialize(weapon);

        // WeaponStateを変更
        Character.SetState(Character.State.SetCurrentWeaponState(WeaponState.None));
        UpdateWeaponVisual();
    }

    /// <summary>
    /// 武器の見た目の更新（腕含む）
    /// </summary>
    private void UpdateWeaponVisual() 
    {
        Character.SetState(Character.State.SetCurrentWeaponState(Character.State.CurrentWeapon.StandBy()));

        // モデルの更新
        string holdingName = $"Arm_{Character.State.CurrentWeapon.Spec.Holding}";
        foreach (var arm in _arms)
        {
            // 全腕共通処理
            bool active = arm.name == holdingName;
            arm.SetActive(active);

            // 「手」の取得
            var hand = arm.transform.GetChildrens().Single().gameObject;
            var handlingWeapon = hand.GetComponent<HandlingWeapon>();

            // ビジュアルのクリア
            handlingWeapon.Clear();

            if (active) // 可視化された腕の処理
            {
                handlingWeapon.Initialize(Character.State.CurrentWeapon);
                CurrentHand = hand.transform;
            }
        }

        // 腕の更新
        if (Character.State.CurrentWeapon.Spec.IsHoldingBody)
        {
            ArmTargetAngleMode();
            Character.SetState(Character.State.SetArmTargetAngle(90));
        }
        else
        {
            Character.SetState(Character.State.SetIsArmLookAtMode(true));
        }

        _onWeaponVisualUpdatedSubject.OnNext(Unit.Default);
    }

    public void KnockBack(Vector3 vector, float magnitude)
    {
        CharacterController.Move(vector * magnitude);
    }

    private bool GetIsGround(Vector3 rayPoint)
    {
        if (CharacterController.isGrounded)
        {
            return true;
        }
        else
        {
            RaycastHit[] hits = Physics.SphereCastAll(rayPoint, CharacterController.radius, Vector3.down, float.MaxValue, LayerMask.GetMask(ConstantsManager.LayerMask_Stage));
            if (hits.Length == 1)
            {
                return Mathf.Abs(hits[0].distance - CharacterController.skinWidth) < Character.GROUNDTHRETHOLD;
            }
            else
            {
                return false;
            }
        }

    }

    private void CameraMuzzleJump(float jumpMagnitude)
    {
        Vector3 rotate = new Vector3(-0.2f, Random.Range(-0.1f, 0.1f), 0) * jumpMagnitude;
        gameObject.transform.Rotate(0, rotate.y, 0);

        _currentXAngle = Mathf.Clamp(_currentXAngle + rotate.x, -_maxAngle, _maxAngle);
        FpsCameraAnchor.transform.localEulerAngles = new Vector3(_currentXAngle, 0, 0);
    }

    public void ArmTargetAngleMode()
    {
        Character.SetState(Character.State.SetIsArmLookAtMode(false));
        _currentArmAngle = ArmBase.localEulerAngles.x;
        if (_currentArmAngle > 180)
        {
            _currentArmAngle -= 360;
        }
    }

    private void ArmMuzzleJump(float jumpMagnitude)
    {
        ArmBase.localEulerAngles = new Vector3(_currentArmAngle - jumpMagnitude, 0, 0);
        _currentArmAngle = ArmBase.localEulerAngles.x;
    }

    private void GrenadeFire(UnityAction<BulletHitInfo> onHitAction)
    {
        var instance = Instantiate(
            CharacterPrefabProvider.Instance.PrefabGrenade,
            FpsCameraAnchor.position + FpsCameraAnchor.forward * 0.5f,
            FpsCameraAnchor.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)));
        var bullet = instance.GetComponent<IBullet>();
        Character.State.CurrentWeapon.Spec.SetBullet(bullet, Team.Empty);
        bullet.OnHit.Subscribe(t => { onHitAction(t.Item2); }).AddTo((MonoBehaviour)bullet);
    }

    private void GunFire(System.Tuple<UnityAction<BulletHitInfo>, float> onHitAction)
    {
        for (short i = 0; i < Character.State.CurrentWeapon.Spec.BulletCount; i++)
        {
            float shootingError = Character.GetShootingError();

            var instance = Instantiate(
                CharacterPrefabProvider.Instance.PrefabBullet,
                FpsCameraAnchor.position,
                FpsCameraAnchor.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)) * Quaternion.Euler(shootingError, 0, 0));
            var bullet = instance.GetComponent<IBullet>();
            Character.State.CurrentWeapon.Spec.SetBullet(bullet, Character.State.Team);
            bullet.OnHit.Subscribe(t => { onHitAction.Item1(t.Item2); }).AddTo((MonoBehaviour)bullet);
        }

        var mfInstance = Instantiate(CharacterPrefabProvider.Instance.PrefabMuzzleFlash, CurrentHand.TransformPoint(-Character.State.CurrentWeapon.Spec.HandlingPosition + Character.State.CurrentWeapon.Spec.MuzzlePosition), CurrentHand.rotation * Quaternion.Euler(0, 180, 0));
        var muzzleFlash = mfInstance.GetComponent<MuzzleFlash>();
        muzzleFlash.FireAudio = CharacterPrefabProvider.Instance.FireAudioClips[$"Assets/Audio/{Character.State.CurrentWeapon.Spec.FireAudio}.wav"];

        Instantiate(CharacterPrefabProvider.Instance.PrefabCartridge, CurrentHand.position, CurrentHand.rotation);

        ArmMuzzleJump(onHitAction.Item2);
    }

    private void ZombieFire(UnityAction<BulletHitInfo> onHitAction)
    {
        var instance = Instantiate(
            CharacterPrefabProvider.Instance.PrefabZombieAttack,
            FpsCameraAnchor.position,
            FpsCameraAnchor.rotation);
        var bullet = instance.GetComponent<IBullet>();
        bullet.Damage = 10;
        bullet.Penetration = 0;
        bullet.Speed = 0;
        bullet.Team = Character.State.Team;
        bullet.OnHit.Subscribe(t => { onHitAction(t.Item2); }).AddTo((MonoBehaviour)bullet);
    }

    private void ChangeAnimation(bool isWaik, bool isRun, float factor)
    {
        _characterAnimator.SetBool(_animatorParamIsWalk, isWaik);
        _characterAnimator.SetBool(_animatorParamIsRun, isRun);
        _characterAnimator.SetFloat(_animatorParamFactor, factor);
    }

    private static readonly float _mouseSensitivity = 150;
    private static readonly float _maxAngle = 85;
    private static readonly string _animatorParamIsWalk = "IsWalk";
    private static readonly string _animatorParamIsRun = "IsRun";
    private static readonly string _animatorParamFactor = "Factor";
    private GameObject[] _arms;
    private VarialbeVisual _visual;
    private Animator _characterAnimator;
    private float _currentXAngle;
    private float _currentArmAngle;
    private Subject<Unit> _onWeaponVisualUpdatedSubject { get; } = new Subject<Unit>();
}
