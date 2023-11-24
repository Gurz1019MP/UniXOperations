using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UniRx;
using System.Collections.Generic;

[RequireComponent(typeof(WeaponSystem))]
[RequireComponent(typeof(ArmController))]
[RequireComponent(typeof(CharacterInputterContainer))]
public class CharacterState : MonoBehaviour
{
    public int ID = -1;
    public float HitPoint = 100;
    public short Team = -1;
    public WeaponState Weapon1 = new WeaponState();
    public WeaponState Weapon2 = new WeaponState();
    public float KillHeight;
    public TextMesh DebugText;



    [ReadOnly]
    public bool IsWeapon1;

    [ReadOnly]
    public bool IsSwitching;

    private GameObject[] _arms;                         // 各持ち方用の腕オブジェクト
    private GameObject _weaponPickup;
    private GameObject _diedCharacter;
    private Mesh upMesh;
    private Material material;
    private Texture2D texture;
    public bool IsZombie;

    #region コンポーネント

    public CharacterInputterContainer InputterContainer { get; private set; }

    public CharacterController CharacterController { get; private set; }

    public FPSMover FPSMover { get; private set; }

    public FPSPointOfViewMover FPSPointOfViewMover { get; private set; }

    public WeaponSystem WeaponSystem { get; private set; }

    public ArmController ArmController { get; private set; }

    public ICharacterInputter Inputter => InputterContainer.Inputter;

    #endregion

    public Transform FpsCameraAnchor { get; private set; }

    public Transform TpsCameraAnchor { get; private set; }

    public Transform Target { get; private set; }

    public Transform UpBase { get; private set; }

    public Transform LegBase { get; private set; }

    public Transform ArmBase { get; private set; }

    public Transform CurrentHand { get; private set; }


    public WeaponState CurrentWeaponState => IsWeapon1 ? Weapon1 : Weapon2;

    public WeaponState DisableWeaponState => IsWeapon1 ? Weapon2 : Weapon1;

    public bool IsReloading => WeaponSystem.IsReloading;

    public CombatStatistics CombatStatistics => WeaponSystem.CombatStatistics;

    #region イベント

    public System.IObservable<CharacterState> OnDied => _onDiedSubject;

    public System.IObservable<Vector3> OnDamaged => _onDamagedSubject;

    public System.IObservable<Vector3> OnClosed => _onClosedSubject;

    public System.IObservable<Vector3> OnGunSound => _onGunSoundSubject;

    public System.IObservable<System.Tuple<bool, WeaponSpec.ZoomModeEnum>> OnChangeZoom => _onChangeZoomSubject;

    public System.IObservable<Unit> OnWeaponVisualUpdated => _onWeaponVisualUpdated;

    #endregion

    void Update()
    {
        if (HitPoint <= 0) Kill();
        if (transform.position.y < KillHeight) Kill();

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
                    WeaponSystem.Reload();
                }
                else if (CurrentWeaponState.Spec.ChangeFireModeWeapon != 0 && Inputter.FireModeEnter)
                {
                    CurrentWeaponState.ChangeFireMode();
                }
                else
                {
                    WeaponSystem.IsTriggerPulled = Inputter.Fire;
                }

                if (CurrentWeaponState.Spec.Scope != WeaponSpec.ZoomModeEnum.None && Inputter.ZoomEnter)
                {
                    ChangeZoom();
                }
            }

            if (IsZombie)
            {
                WeaponSystem.IsTriggerPulled = Inputter.Fire;
            }

            if (Inputter.SwitchWeaponEnter && !IsReloading && !IsSwitching)
            {
                //Debug.Log("SwitchWeapon");
                StartCoroutine(SwitchWeapon());
            }
        }
    }

    public void InitCharacterState(PointData pointData, CharacterInfomation info, bool equipWeapon2)
    {
        // 初期化
        // コンポーネントの取得
        InputterContainer = GetComponent<CharacterInputterContainer>();
        CharacterController = GetComponent<CharacterController>();
        FPSMover = GetComponent<FPSMover>();
        FPSPointOfViewMover = GetComponent<FPSPointOfViewMover>();
        WeaponSystem = GetComponent<WeaponSystem>();
        ArmController = GetComponent<ArmController>();

        InitHierarchyObject();

        // インスタンス化プレハブの取得
        _weaponPickup = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabWeaponPickup);
        _diedCharacter = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabDiedCharacter);

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

        // マズルジャンプの設定
        WeaponSystem.MuzzleJumpAction = FPSPointOfViewMover.MuzzleJump;
        WeaponSystem.UpdateWeaponVisualAction = UpdateWeaponVisual;

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
            material = AssetLoader.LoadAsset<Material>(ConstantsManager.GetResoucePathCharacterMaterial);
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

    private void Kill()
    {
        DropWeapon(Weapon1, true);
        DropWeapon(Weapon2, true);

        var instance = Instantiate(_diedCharacter, transform.position + Vector3.up * -0.558f, transform.rotation);
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
    public IEnumerator SwitchWeapon()
    {
        IsSwitching = true;

        if (WeaponSystem.isZoom) ChangeZoom();
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
            CurrentWeaponState.ChangeWeapon(pickedWeapon);
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
    public void DropWeapon(WeaponState weapon, bool isRandom = false)
    {
        if (WeaponSystem.isZoom) ChangeZoom();

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
        var instance = Instantiate(_weaponPickup, InstantiatePosition, transform.rotation * Quaternion.Euler(0, 0, -90));

        // ピックアップを初期化
        var pickup = instance.GetComponent<WeaponPickup>();
        pickup.Initialize(weapon);

        // WeaponStateを変更
        weapon.ChangeWeapon(WeaponState.None);
        UpdateWeaponVisual();
    }

    public void ChangeZoom()
    {
        WeaponSystem.isZoom = !WeaponSystem.isZoom;
        _onChangeZoomSubject.OnNext(new System.Tuple<bool, WeaponSpec.ZoomModeEnum>(WeaponSystem.isZoom, CurrentWeaponState.Spec.Scope));
    }

    /// <summary>
    /// 武器の見た目の更新（腕含む）
    /// </summary>
    private void UpdateWeaponVisual()
    {
        // WeaponSystemの初期化
        WeaponSystem.SetWeapon(CurrentWeaponState);

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

        if (CurrentWeaponState.Spec.Holding == WeaponSpec.HoldingMethod.Body)
        {
            ArmController.TargetAngleMode();
            ArmController.TargetAngle = 90;
        }
        else
        {
            ArmController.LookAtMode();
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
        //Debug.Log($"TakeDamage (Id:{ID}, HitPoint:{HitPoint})");
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

    private Subject<CharacterState> _onDiedSubject = new Subject<CharacterState>();
    private Subject<Vector3> _onDamagedSubject = new Subject<Vector3>();
    private Subject<Vector3> _onClosedSubject = new Subject<Vector3>();
    private Subject<Vector3> _onGunSoundSubject = new Subject<Vector3>();
    private Subject<System.Tuple<bool, WeaponSpec.ZoomModeEnum>> _onChangeZoomSubject = new Subject<System.Tuple<bool, WeaponSpec.ZoomModeEnum>>();
    private Subject<Unit> _onWeaponVisualUpdated = new Subject<Unit>();
}
