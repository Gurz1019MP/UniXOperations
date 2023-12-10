using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CharacterInputterContainer))]
public class Character : MonoBehaviour
{
    public int ID = -1;
    public float HitPoint = 100;
    public short Team = -1;
    public WeaponState Weapon1 = new WeaponState();
    public WeaponState Weapon2 = new WeaponState();
    public float KillHeight;
    public TextMesh DebugText;
    public float WalkSpeed;        // 一般的な成人の歩行スピード
    public float RunMultiplier;    // 走ったときの速度係数
    public float Gravity;          // 重力
    public float JumpPower;        // ジャンプ力
    public float SlidingForce;     // スライディング力
    public float GroundThreshold = 0.01f;
    public float ArmRotateSpeed;
    public float ArmLookAtSpeed;

    #region Readonlyフィールド

    [ReadOnly]
    public bool IsWeapon1;

    [ReadOnly]
    public bool IsSwitching;

    [ReadOnly]
    public bool IsZombie;

    [ReadOnly]
    public float ArmTargetAngle;

    [ReadOnly]
    public bool IsArmLookAtMode;

    [ReadOnly]
    public bool IsTriggerPulled;

    [ReadOnly]
    public bool IsSemiFired;

    [ReadOnly]
    public bool IsLoading;

    [ReadOnly]
    public bool IsReloading;

    [ReadOnly]
    public float AccuracyRatio = 0;

    [ReadOnly]
    public bool IsZoom;

    #endregion

    #region privateフィールド

    private GameObject[] _arms;                         // 各持ち方用の腕オブジェクト
    private Mesh upMesh;
    private Material material;
    private Texture2D texture;
    private float _currentXAngle;
    private Vector3 _moveDelta;
    private Animator _characterAnimator;
    private float _animatorFactor = 0.0f;
    private float _currentArmAngle;
    private AnimationCurve Accuracy;

    #endregion

    #region コンポーネント

    public CharacterInputterContainer InputterContainer { get; private set; }

    public CharacterController CharacterController { get; private set; }

    public InputterBase Inputter => InputterContainer.Inputter;

    #endregion

    #region プロパティ

    public Transform FpsCameraAnchor { get; private set; }

    public Transform TpsCameraAnchor { get; private set; }

    public Transform Target { get; private set; }

    public Transform UpBase { get; private set; }

    public Transform LegBase { get; private set; }

    public Transform ArmBase { get; private set; }

    public Transform CurrentHand { get; private set; }


    public WeaponState CurrentWeaponState => IsWeapon1 ? Weapon1 : Weapon2;

    public WeaponState DisableWeaponState => IsWeapon1 ? Weapon2 : Weapon1;

    public bool IsGround { get; private set; }

    public float MinAccuracyRatio { get; set; }

    public float ShootingErrorGroundFactorMax => IsGround ? 0 : 3;

    public float ShootingErrorMoveFactorMax => Mathf.Abs(Inputter.Horizontal) > 0 || Mathf.Abs(Inputter.Vertical) > 0 ? 1.5f : 0;

    public float ShootingErrorWalkFactorMax => Inputter.Walk ? 1f : 0;

    public float ShootingErrorHealthFactorMax => (100 - Mathf.Clamp(HitPoint, 0, 100)) / 100 * 1f;

    public float MaxShootingError
    {
        get
        {
            return ShootingErrorGroundFactorMax +
                   ShootingErrorMoveFactorMax +
                   ShootingErrorWalkFactorMax +
                   ShootingErrorHealthFactorMax +
                   Accuracy.Evaluate(AccuracyRatio);
        }
    }

    public CombatStatistics CombatStatistics { get; set; }

    #endregion

    #region イベント

    public System.IObservable<Character> OnDied => _onDiedSubject;

    public System.IObservable<Vector3> OnDamaged => _onDamagedSubject;

    public System.IObservable<Vector3> OnClosed => _onClosedSubject;

    public System.IObservable<Vector3> OnGunSound => _onGunSoundSubject;

    public System.IObservable<System.Tuple<bool, WeaponSpec.ZoomModeEnum>> OnChangeZoom => _onChangeZoomSubject;

    public System.IObservable<Unit> OnWeaponVisualUpdated => _onWeaponVisualUpdated;

    public System.IObservable<Unit> OnFire => _onFireSubject;

    public System.IObservable<BulletHitInfo> OnHit => _onHitSubject;

    #endregion

    public void InitCharacterState(PointData pointData, CharacterInfomation info, bool equipWeapon2)
    {
        // 初期化
        // コンポーネントの取得
        InputterContainer = GetComponent<CharacterInputterContainer>();
        CharacterController = GetComponent<CharacterController>();
        _characterAnimator = GetComponentInChildren<Animator>();

        InitHierarchyObject();

        // パラメータの設定
        ID = pointData.Data4;
        HitPoint = info.Spec.Hitpoint;
        Team = info.Team;
        IsZombie = info.Spec.IsZombie;
        EquipWeapon(Weapon1, info.Spec.Weapon1);
        if (equipWeapon2)
        {
            EquipWeapon(Weapon2, info.Spec.Weapon2);
        }

        // 初期武器を装備（WeaponSystemの初期化）
        UpdateWeaponVisual();

        // モデルの読み込み
        if (!string.IsNullOrEmpty(info.Spec.UpperModel) && !string.IsNullOrEmpty(info.Spec.Texture))
        {
            // ビジュアル用オブジェクトを取得
            var arms = GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagArmHolding)).Select(t => t.gameObject).ToArray();
            var up = GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagUp)).Select(t => t.gameObject).Single();
            var leg = GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagLeg)).Select(t => t.gameObject).Single();

            // メッシュの読み込み
            upMesh = AssetLoader.LoadAsset<Mesh>(ConstantsManager.GetResoucePathCharacterModel(info.Spec.UpperModel));
            VisualChanger.ChangeMesh(up, upMesh);

            // マテリアルの読み込み
            material = AssetLoader.LoadAsset<Material>(ConstantsManager.GetResoucePathCharacterMaterial());
            material.shader = Shader.Find("Unlit/Texture");
            texture = AssetLoader.LoadAsset<Texture2D>(ConstantsManager.GetResoucePathCharacterTexture(info.Spec.Texture));
            foreach (var target in arms.Concat(new GameObject[] { up, leg }))
            {
                VisualChanger.ChangeMaterial(target, material, texture);
            }
        }
        else
        {
            Debug.Log("人情報に上半身モデル情報が登録されていません");
        }

        _currentXAngle = FpsCameraAnchor.transform.localEulerAngles.x;

        // 武器系統の初期化
        CombatStatistics = new CombatStatistics();
        OnFire.Subscribe(CombatStatistics.OnFire).AddTo(gameObject);
        OnHit.Subscribe(CombatStatistics.OnHit).AddTo(gameObject);
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
        if (HitPoint <= 0) Kill();
        if (transform.position.y < KillHeight) HitPoint = 0;

        _moveDelta.x = 0;
        _moveDelta.z = 0;

        Vector3 rayPoint = transform.position + CharacterController.center + Vector3.up * (-CharacterController.height * 0.5F + CharacterController.radius);
        bool newIsGround = GetIsGround(rayPoint);
        RaycastHit[] slideHits = Physics.RaycastAll(rayPoint, Vector3.down, float.MaxValue, LayerMask.GetMask("Stage"));
        float tilt = slideHits.Length == 1 && newIsGround ? Mathf.Acos(Vector3.Dot(slideHits[0].normal, Vector3.up)) : 0;
        bool isSlide = tilt > CharacterController.slopeLimit / 180F * Mathf.PI;

        // 着地判定
        if (!IsGround && newIsGround)
        {
            // 落下ダメージ
            if (_moveDelta.y < -8)
            {
                TakeDamage((_moveDelta.y + 7) * -50, Vector3.zero);
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
                _moveDelta.y = JumpPower;
                IsGround = false;
            }
            else
            {
                _moveDelta.y = -Gravity * Time.deltaTime;
            }
        }
        else
        {
            _moveDelta.y -= Gravity * Time.deltaTime;
        }

        if (Inputter != null)
        {
            if (CurrentWeaponState.Kind != 0)
            {
                if (Inputter.DropWeaponEnter && !IsReloading && !IsSwitching)
                {
                    DropWeapon(CurrentWeaponState);
                }
                else if (CurrentWeaponState.Ammo > 0 && Inputter.ReloadEnter && !IsReloading && !IsSwitching)
                {
                    Reload();
                }
                else if (CurrentWeaponState.Spec.ChangeFireModeWeapon != 0 && Inputter.FireModeEnter)
                {
                    CurrentWeaponState.ChangeFireMode();
                }
                else
                {
                    IsTriggerPulled = Inputter.Fire;
                }

                if (CurrentWeaponState.Spec.Scope != WeaponSpec.ZoomModeEnum.None && Inputter.ZoomEnter)
                {
                    ChangeZoom();
                }
            }

            if (IsZombie)
            {
                IsTriggerPulled = Inputter.Fire;
            }

            if (Inputter.SwitchWeaponEnter && !IsReloading && !IsSwitching)
            {
                //Debug.Log("SwitchWeapon");
                StartCoroutine(SwitchWeapon());
            }

            if (Inputter.Walk)
            {
                var inputDelta = Vector3.forward * WalkSpeed;
                inputDelta = transform.TransformDirection(inputDelta);

                _moveDelta.x += inputDelta.x;
                _moveDelta.z += inputDelta.z;

                _characterAnimator.SetBool(_animatorParamIsWalk, true);
                _characterAnimator.SetBool(_animatorParamIsRun, false);
                _animatorFactor = 1.0f;
            }
            else
            {
                var inputDelta = new Vector3(Inputter.Horizontal, 0, Inputter.Vertical).normalized * WalkSpeed;

                if (inputDelta.z == 0)
                {
                    _characterAnimator.SetBool(_animatorParamIsWalk, false);
                    _characterAnimator.SetBool(_animatorParamIsRun, false);
                    _animatorFactor = 0;
                }
                else if (inputDelta.z > 0)
                {
                    inputDelta.z *= RunMultiplier;

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
            Vector3 reactionForce = slideHits[0].normal * SlidingForce;

            _moveDelta.x += reactionForce.x;
            _moveDelta.y -= reactionForce.y;
            _moveDelta.z += reactionForce.z;
        }

        CharacterController.Move(_moveDelta * Time.deltaTime);

        // 視点移動
        Vector3 rotate = new Vector3(Inputter.MouseY, Inputter.MouseX, 0) * _mouseSensitivity * Time.deltaTime;
        gameObject.transform.Rotate(0, rotate.y, 0);

        _currentXAngle = Mathf.Clamp(_currentXAngle + rotate.x, -_maxAngle, _maxAngle);
        FpsCameraAnchor.transform.localEulerAngles = new Vector3(_currentXAngle, 0, 0);

        // 腕の角度の制御
        if (IsArmLookAtMode)
        {
            Vector3 direction = Target.transform.position - ArmBase.position;
            float YZAngle = Vector3.SignedAngle(ArmBase.forward, direction, ArmBase.right);
            _currentArmAngle += YZAngle * ArmLookAtSpeed * Time.deltaTime;
            ArmBase.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
        }
        else
        {
            if (Mathf.Abs(_currentArmAngle - ArmTargetAngle) > 1f)
            {
                float direction = _currentArmAngle < ArmTargetAngle ? 1 : -1;
                float rotateSpeed = ArmRotateSpeed * direction * Time.deltaTime;
                if (rotateSpeed > Mathf.Abs(ArmTargetAngle - _currentArmAngle))
                {
                    rotateSpeed = ArmTargetAngle - _currentArmAngle;
                }
                _currentArmAngle += rotateSpeed;
                ArmBase.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
            }
        }

        // 武器系統
        if (IsZombie)
        {
            if (IsTriggerPulled && !IsLoading)
            {
                ZombieFire();
            }
        }
        else
        {
            if (IsTriggerPulled && !IsLoading && CurrentWeaponState.Magazine > 0 && !IsReloading && !IsSemiFired)
            {
                Fire();
            }

            if (IsSemiFired && !IsTriggerPulled)
            {
                IsSemiFired = false;
            }

            float ShotgunValue = CurrentWeaponState.Spec.BulletCount > 1 ? 0.5f : 0;
            if (CurrentWeaponState.Spec.Scope == WeaponSpec.ZoomModeEnum.Detail && !IsZoom)
            {
                ShotgunValue = 1f;
            }
            MinAccuracyRatio = ShotgunValue;

            AccuracyRatio = Mathf.Clamp(AccuracyRatio - 1f * Time.deltaTime, MinAccuracyRatio, 1);
        }
    }

    private void Kill()
    {
        HitPoint = 0;
        DropWeapon(Weapon1, true);
        DropWeapon(Weapon2, true);

        var instance = Instantiate(CharacterPrefabProvider.Instance.PrefabDiedCharacter, transform.position + Vector3.up * -0.558f, transform.rotation);
        var diedCharacter = instance.GetComponent<DiedCharacter>();
        diedCharacter.Initialize(upMesh, material, texture, false);

        _onDiedSubject.OnNext(this);
        _onDiedSubject.OnCompleted();

        Destroy(gameObject);
        //Debug.Log(string.Format("ID:{0}は死亡しました", ID));
    }

    /// <summary>
    /// 武器の切り替え
    /// </summary>
    private IEnumerator SwitchWeapon()
    {
        IsSwitching = true;

        if (IsZoom) ChangeZoom();
        yield return new WaitForSeconds(0.2f);

        IsWeapon1 = !IsWeapon1;
        UpdateWeaponVisual();

        IsSwitching = false;
    }

    /// <summary>
    /// 武器を拾う
    /// </summary>
    /// <param name="pickedWeapon">拾った武器</param>
    public bool PickupWeapon(WeaponState pickedWeapon)
    {
        if (CurrentWeaponState.Kind == 0 && !IsZombie)
        {
            ChangeWeapon(pickedWeapon);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ChangeWeapon(WeaponState pickedWeapon)
    {
        CurrentWeaponState.ChangeWeapon(pickedWeapon);
        UpdateWeaponVisual();
    }

    /// <summary>
    /// 武器を捨てる
    /// </summary>
    private void DropWeapon(WeaponState weapon, bool isRandom = false)
    {
        if (IsZoom) ChangeZoom();

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
        var instance = Instantiate(CharacterPrefabProvider.Instance.PrefabWeaponPickup, InstantiatePosition, transform.rotation * Quaternion.Euler(0, 0, -90));

        // ピックアップを初期化
        var pickup = instance.GetComponent<WeaponPickup>();
        pickup.Initialize(weapon);

        // WeaponStateを変更
        weapon.ChangeWeapon(WeaponState.None);
        UpdateWeaponVisual();
    }

    private void ChangeZoom()
    {
        IsZoom = !IsZoom;
        _onChangeZoomSubject.OnNext(new System.Tuple<bool, WeaponSpec.ZoomModeEnum>(IsZoom, CurrentWeaponState.Spec.Scope));
    }

    /// <summary>
    /// 武器の見た目の更新（腕含む）
    /// </summary>
    private void UpdateWeaponVisual()
    {
        // フィールドの更新
        IsSemiFired = false;
        IsLoading = false;

        // 射撃精度のカーブ
        Accuracy = new AnimationCurve();
        Accuracy.AddKey(0, CurrentWeaponState.Spec.AccuracyMin);
        Accuracy.AddKey(1, CurrentWeaponState.Spec.AccuracyMax);

        // モデルの更新
        string holdingName = $"Arm_{CurrentWeaponState.Spec.Holding}";
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
                handlingWeapon.Initialize(CurrentWeaponState.Spec);
                CurrentHand = hand.transform;
            }
        }

        // 腕の更新
        if (CurrentWeaponState.Spec.Holding == WeaponSpec.HoldingMethod.Body)
        {
            ArmTargetAngleMode();
            ArmTargetAngle = 90;
        }
        else
        {
            ArmLookAtMode();
        }

        _onWeaponVisualUpdated.OnNext(Unit.Default);
    }

    private void EquipWeapon(WeaponState weaponState, short weaponKind)
    {
        if (weaponKind == 0) return;

        weaponState.Kind = weaponKind;
        weaponState.Magazine = WeaponSpec.GetSpec(weaponKind).MagazineSize;
        weaponState.Ammo = WeaponSpec.GetSpec(weaponKind).MagazineSize * 2;
    }

    public BulletHitInfo TakeDamage(float baseDamage, RaycastHit hit, Vector3 direction)
    {
        BulletHitInfo.HitPartEnum hitPart = CheckHitPart(hit);
        float damage = baseDamage * getDamageCoefficient(hitPart);

        return TakeDamage(damage, direction, hitPart);
    }

    public BulletHitInfo TakeDamage(float damage, Vector3 direction, BulletHitInfo.HitPartEnum hitPart = BulletHitInfo.HitPartEnum.None)
    {
        HitPoint -= damage;
        _onDamagedSubject.OnNext(direction);

        return new BulletHitInfo(hitPart, HitPoint <= 0);
    }

    public void KnockBack(Vector3 vector, float magnitude)
    {
        CharacterController.Move(vector * magnitude);
    }

    private BulletHitInfo.HitPartEnum CheckHitPart(RaycastHit hit)
    {
        if (hit.collider.CompareTag(ConstantsManager.TagUp))
        {
            if (hit.point.y - hit.transform.position.y > 1.0f)
            {
                //Debug.Log("Head shot");
                return BulletHitInfo.HitPartEnum.Head;
            }
            else
            {
                return BulletHitInfo.HitPartEnum.Body;
            }
        }
        else if (hit.collider.CompareTag(ConstantsManager.TagLeg))
        {
            return BulletHitInfo.HitPartEnum.Leg;
        }
        else if (hit.collider.CompareTag(ConstantsManager.TagArmHolding))
        {
            return BulletHitInfo.HitPartEnum.Arm;
        }
        else
        {
            return BulletHitInfo.HitPartEnum.None;
        }
    }

    private float getDamageCoefficient(BulletHitInfo.HitPartEnum hitPart)
    {
        switch (hitPart)
        {
            case BulletHitInfo.HitPartEnum.Body:
                return 1;
            case BulletHitInfo.HitPartEnum.Head:
                return 2;
            case BulletHitInfo.HitPartEnum.Leg:
                return 0.6f;
            case BulletHitInfo.HitPartEnum.Arm:
                return 0.4f;
            case BulletHitInfo.HitPartEnum.None:
            default:
                return 1;
        }
    }

    public void RaiseOnClose(Vector3 direction)
    {
        _onClosedSubject.OnNext(direction);
    }

    public void RaiseOnGunSound(Vector3 direction)
    {
        _onGunSoundSubject.OnNext(direction);
    }

    public void ResetMoveDeltaY()
    {
        _moveDelta.y = 0.0f;
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
                return Mathf.Abs(hits[0].distance - CharacterController.skinWidth) < GroundThreshold;
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

    private void ArmLookAtMode()
    {
        IsArmLookAtMode = true;
    }

    public void ArmTargetAngleMode()
    {
        IsArmLookAtMode = false;
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

    private void Fire()
    {
        if (CurrentWeaponState.Spec.IsGrenade)
        {
            var instance = Instantiate(
                CharacterPrefabProvider.Instance.PrefabGrenade,
                FpsCameraAnchor.position + FpsCameraAnchor.forward * CurrentWeaponState.Spec.BulletInstanceOffset,
                FpsCameraAnchor.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)));
            var bullet = instance.GetComponent<IBullet>();
            bullet.Damage = CurrentWeaponState.Spec.FirePower;
            bullet.Speed = CurrentWeaponState.Spec.BulletSpeed;
            bullet.OnHit.Subscribe(t =>
            {
                _onHitSubject.OnNext(t.Item2);
            }).AddTo((MonoBehaviour)bullet);

            _onFireSubject.OnNext(Unit.Default);

            StartCoroutine(Loading());

            if (CurrentWeaponState.Ammo > 0)
            {
                Reload();
            }

            if (CurrentWeaponState.Ammo == 0 && CurrentWeaponState.Magazine == 0)
            {
                CurrentWeaponState.LostWeapon();
                UpdateWeaponVisual();
            }
        }
        else
        {
            for (short i = 0; i < CurrentWeaponState.Spec.BulletCount; i++)
            {
                float shootingError = GetShootingError();

                var instance = Instantiate(
                    CharacterPrefabProvider.Instance.PrefabBullet,
                    FpsCameraAnchor.position + FpsCameraAnchor.forward * CurrentWeaponState.Spec.BulletInstanceOffset,
                    FpsCameraAnchor.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)) * Quaternion.Euler(shootingError, 0, 0));
                var bullet = instance.GetComponent<IBullet>();
                bullet.Damage = CurrentWeaponState.Spec.FirePower;
                bullet.Penetration = CurrentWeaponState.Spec.PenetrationPower;
                bullet.Speed = CurrentWeaponState.Spec.BulletSpeed;
                bullet.Team = Team;
                bullet.Sound = CurrentWeaponState.Spec.Sound;
                bullet.OnHit.Subscribe(t =>
                {
                    _onHitSubject.OnNext(t.Item2);
                }).AddTo((MonoBehaviour)bullet);
            }

            _onFireSubject.OnNext(Unit.Default);

            if (IsZoom)
            {
                CameraMuzzleJump(CurrentWeaponState.Spec.FirePower * 0.1f);
            }

            ArmMuzzleJump(CurrentWeaponState.Spec.MuzzleJump);

            var mfInstance = Instantiate(CharacterPrefabProvider.Instance.PrefabMuzzleFlash, CurrentHand.TransformPoint(-CurrentWeaponState.Spec.HandlingPosition + CurrentWeaponState.Spec.MuzzlePosition), CurrentHand.rotation * Quaternion.Euler(0, 180, 0));
            var muzzleFlash = mfInstance.GetComponent<MuzzleFlash>();
            muzzleFlash.FireAudio = CharacterPrefabProvider.Instance.FireAudioClips[$"Assets/Audio/{CurrentWeaponState.Spec.FireAudio}.wav"];

            Instantiate(CharacterPrefabProvider.Instance.PrefabCartridge, CurrentHand.position, CurrentHand.rotation);

            // 射撃誤差を増加
            if (AccuracyRatio < 1)
            {
                AccuracyRatio = Mathf.Clamp(AccuracyRatio + 0.2f, MinAccuracyRatio, 1);
            }

            StartCoroutine(Loading());

            if (!CurrentWeaponState.Spec.FullAuto)
            {
                IsSemiFired = true;
            }
        }
    }

    private float GetShootingError()
    {
        return Random.Range(0, ShootingErrorGroundFactorMax) +
               Random.Range(0, ShootingErrorMoveFactorMax) +
               Random.Range(0, ShootingErrorWalkFactorMax) +
               Random.Range(0, ShootingErrorHealthFactorMax) +
               Accuracy.Evaluate(Random.Range(0, AccuracyRatio));
    }

    private IEnumerator Loading()
    {
        CurrentWeaponState.Magazine--;
        IsLoading = true;
        yield return new WaitForSeconds(CurrentWeaponState.Spec.FiringInterval);
        IsLoading = false;
    }

    private void Reload()
    {
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        IsReloading = true;
        yield return new WaitForSeconds(CurrentWeaponState.Spec.ReloadTime);
        CurrentWeaponState.Magazine = CurrentWeaponState.Ammo > CurrentWeaponState.Spec.MagazineSize ? CurrentWeaponState.Spec.MagazineSize : CurrentWeaponState.Ammo;
        CurrentWeaponState.Ammo -= CurrentWeaponState.Magazine;
        IsReloading = false;
    }

    private void ZombieFire()
    {
        var instance = Instantiate(
            CharacterPrefabProvider.Instance.PrefabZombieAttack,
            FpsCameraAnchor.position + FpsCameraAnchor.forward * CurrentWeaponState.Spec.BulletInstanceOffset,
            FpsCameraAnchor.rotation);
        var bullet = instance.GetComponent<IBullet>();
        bullet.Damage = 10;
        bullet.Penetration = 0;
        bullet.Speed = 0;
        bullet.Team = Team;
        bullet.OnHit.Subscribe(t =>
        {
            _onHitSubject.OnNext(t.Item2);
        }).AddTo((MonoBehaviour)bullet);

        _onFireSubject.OnNext(Unit.Default);

        StartCoroutine(ZombieLoading());
    }

    private IEnumerator ZombieLoading()
    {
        IsLoading = true;
        yield return new WaitForSeconds(1);
        IsLoading = false;
    }

    private static readonly float _mouseSensitivity = 150;
    private static readonly float _maxAngle = 85;
    private static readonly string _animatorParamIsWalk = "IsWalk";
    private static readonly string _animatorParamIsRun = "IsRun";
    private static readonly string _animatorParamFactor = "Factor";
    private Subject<Character> _onDiedSubject = new Subject<Character>();
    private Subject<Vector3> _onDamagedSubject = new Subject<Vector3>();
    private Subject<Vector3> _onClosedSubject = new Subject<Vector3>();
    private Subject<Vector3> _onGunSoundSubject = new Subject<Vector3>();
    private Subject<System.Tuple<bool, WeaponSpec.ZoomModeEnum>> _onChangeZoomSubject = new Subject<System.Tuple<bool, WeaponSpec.ZoomModeEnum>>();
    private Subject<Unit> _onWeaponVisualUpdated = new Subject<Unit>();
    private Subject<Unit> _onFireSubject = new Subject<Unit>();
    private Subject<BulletHitInfo> _onHitSubject = new Subject<BulletHitInfo>();
}
