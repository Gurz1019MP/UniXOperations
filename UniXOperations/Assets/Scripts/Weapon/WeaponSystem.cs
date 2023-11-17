using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

[System.Serializable]
public class WeaponSystem : MonoBehaviour
{
    public GameObject MuzzleFlash;

    [ReadOnly]
    public WeaponState Weapon;
    //[ReadOnly]
    //public short Team = -1;
    //[ReadOnly]
    //public bool IsZombie;

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
    public CharacterState CharacterState;

    [ReadOnly]
    public FPSMover FPSMover;

    [ReadOnly]
    public Transform Hand;

    [ReadOnly]
    public bool isZoom;

    private Subject<Unit> _onFireSubject = new Subject<Unit>();
    public System.IObservable<Unit> OnFire => _onFireSubject;

    private Subject<BulletHitInfo> _onHitSubject = new Subject<BulletHitInfo>();
    public System.IObservable<BulletHitInfo> OnHit => _onHitSubject;

    private AnimationCurve Accuracy;
    private Dictionary<string, GameObject> _bulletPrefabs;
    private Transform _firePositionTransform;
    private GameObject _cartridgePrefab;

    private Dictionary<string, AudioClip> fireAudioClips;

    public float MinAccuracyRatio { get; set; }

    public float ShootingErrorGroundFactorMax
    {
        get { return FPSMover.IsGround ? 0 : 3; }
    }

    public float ShootingErrorMoveFactorMax
    {
        get { return Mathf.Abs(FPSMover.Inputter.Horizontal) > 0 || Mathf.Abs(FPSMover.Inputter.Vertical) > 0 ? 1.5f : 0; }
    }

    public float ShootingErrorWalkFactorMax
    {
        get { return FPSMover.Inputter.Walk ? 1f : 0; }
    }

    public float ShootingErrorHealthFactoMax
    {
        get { return (100 - Mathf.Clamp(CharacterState.HitPoint, 0, 100)) / 100 * 1f; }
    }

    public float MaxShootingError
    {
        get
        {
            return ShootingErrorGroundFactorMax +
                   ShootingErrorMoveFactorMax +
                   ShootingErrorWalkFactorMax +
                   ShootingErrorHealthFactoMax +
                   Accuracy.Evaluate(AccuracyRatio);
        }
    }

    public CombatStatistics CombatStatistics { get; set; }

    public System.Action<float> MuzzleJumpAction { get; set; }

    public System.Action UpdateWeaponVisualAction { get; set; }

    public void Initialize(CharacterState characterState)
    {
        _bulletPrefabs = ConstantsManager.BulletPrefabs.ToDictionary(bp => bp, bp => AssetLoader.LoadAsset<GameObject>(bp));
        _firePositionTransform = transform.GetChildrens().Single(c => c.CompareTag(ConstantsManager.TagFpsCameraAnchor));
        _cartridgePrefab = AssetLoader.LoadAsset<GameObject>(ConstantsManager.CartridgePrefab);
        CharacterState = characterState;
        FPSMover = characterState.FPSMover;
        fireAudioClips = ConstantsManager.FireAudios.ToDictionary(c => c, c => AssetLoader.LoadAsset<AudioClip>(c));
        CombatStatistics = new CombatStatistics();
        OnFire.Subscribe(CombatStatistics.OnFire).AddTo(gameObject);
        OnHit.Subscribe(CombatStatistics.OnHit).AddTo(gameObject);
    }

    public void SetWeapon(WeaponState weaponState)
    {
        Weapon = weaponState;
        IsSemiFired = false;
        IsLoading = false;

        // 射撃精度のカーブ
        Accuracy = new AnimationCurve();
        Accuracy.AddKey(0, Weapon.Spec.AccuracyMin);
        Accuracy.AddKey(1, Weapon.Spec.AccuracyMax);

        var characterState = GetComponent<CharacterState>();
        characterState.OnWeaponVisualUpdated.Subscribe(_ =>
        {
            Hand = characterState.CurrentHand;
        }).AddTo(characterState);
    }

    private void Update()
    {
        if (CharacterState.IsZombie)
        {
            if (IsTriggerPulled && !IsLoading)
            {
                ZombieFire();
            }
        }
        else
        {
            if (IsTriggerPulled && !IsLoading && Weapon.Magazine > 0 && !IsReloading && !IsSemiFired)
            {
                Fire();
            }

            if (IsSemiFired && !IsTriggerPulled)
            {
                IsSemiFired = false;
            }

            float ShotgunValue = Weapon.Spec.BulletCount > 1 ? 0.5f : 0;
            if (Weapon.Spec.Scope == WeaponSpec.ZoomModeEnum.Detail && !isZoom)
            {
                ShotgunValue = 1f;
            }
            MinAccuracyRatio = ShotgunValue;

            AccuracyRatio = Mathf.Clamp(AccuracyRatio - 1f * Time.deltaTime, MinAccuracyRatio, 1);
        }
    }

    private void Fire()
    {
        for (short i = 0; i < Weapon.Spec.BulletCount; i++)
        {
            float shootingError = GetShootingError();

            var instance = Instantiate(
                _bulletPrefabs[$"Assets/Prefabs/{Weapon.Spec.BulletName}.prefab"],
                _firePositionTransform.position + _firePositionTransform.forward * Weapon.Spec.BulletInstanceOffset,
                _firePositionTransform.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)) * Quaternion.Euler(shootingError, 0, 0));
            var bullet = instance.GetComponent<IBullet>();
            bullet.Damage = Weapon.Spec.FirePower;
            bullet.Penetration = Weapon.Spec.PenetrationPower;
            bullet.Speed = Weapon.Spec.BulletSpeed;
            bullet.Team = CharacterState.Team;
            bullet.Sound = Weapon.Spec.Sound;
            bullet.OnHit.Subscribe(t =>
            {
                _onHitSubject.OnNext(t.Item2);
            }).AddTo((MonoBehaviour)bullet);
        }

        _onFireSubject.OnNext(Unit.Default);

        if (MuzzleJumpAction != null && isZoom)
        {
            MuzzleJumpAction(Weapon.Spec.FirePower * 0.1f);
        }

        if (!Weapon.Spec.IsGrenade)
        {
            //var mfInstance = Instantiate(MuzzleFlash, Hand.position + Hand.forward * 0.1f, Hand.rotation * Quaternion.Euler(0, 180, 0));
            var mfInstance = Instantiate(MuzzleFlash, Hand.TransformPoint(-Weapon.Spec.HandlingPosition + Weapon.Spec.MuzzlePosition), Hand.rotation * Quaternion.Euler(0, 180, 0));
            var muzzleFlash = mfInstance.GetComponent<MuzzleFlash>();
            muzzleFlash.FireAudio = fireAudioClips[$"Assets/Audio/{Weapon.Spec.FireAudio}.wav"];

            Instantiate(_cartridgePrefab, Hand.position, Hand.rotation);
        }

        // 射撃誤差を増加
        if (AccuracyRatio < 1)
        {
            AccuracyRatio = Mathf.Clamp(AccuracyRatio + 0.2f, MinAccuracyRatio, 1);
        }

        StartCoroutine(Loading());

        if (!Weapon.Spec.FullAuto)
        {
            IsSemiFired = true;
        }

        if (Weapon.Spec.IsGrenade)
        {
            if (Weapon.Ammo > 0)
            {
                Reload();
            }

            if (Weapon.Ammo == 0 && Weapon.Magazine == 0)
            {
                Weapon.LostWeapon();
                UpdateWeaponVisualAction();
            }
        }
    }

    private float GetShootingError()
    {
        return Random.Range(0, ShootingErrorGroundFactorMax) +
               Random.Range(0, ShootingErrorMoveFactorMax) +
               Random.Range(0, ShootingErrorWalkFactorMax) +
               Random.Range(0, ShootingErrorHealthFactoMax) +
               Accuracy.Evaluate(Random.Range(0, AccuracyRatio));
    }

    private IEnumerator Loading()
    {
        Weapon.Magazine--;
        IsLoading = true;
        yield return new WaitForSeconds(Weapon.Spec.FiringInterval);
        IsLoading = false;
    }

    public void Reload()
    {
        StartCoroutine(Reloading());
    }

    private IEnumerator Reloading()
    {
        IsReloading = true;
        yield return new WaitForSeconds(Weapon.Spec.ReloadTime);
        Weapon.Magazine = Weapon.Ammo > Weapon.Spec.MagazineSize ? Weapon.Spec.MagazineSize : Weapon.Ammo;
        Weapon.Ammo -= Weapon.Magazine;
        IsReloading = false;
    }

    private void ZombieFire()
    {
        var instance = Instantiate(
            _bulletPrefabs[ConstantsManager.PrefabZombieAttack],
            _firePositionTransform.position + _firePositionTransform.forward * Weapon.Spec.BulletInstanceOffset,
            _firePositionTransform.rotation);
        var bullet = instance.GetComponent<IBullet>();
        bullet.Damage = 10;
        bullet.Penetration = 0;
        bullet.Speed = 0;
        bullet.Team = CharacterState.Team;
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
}
