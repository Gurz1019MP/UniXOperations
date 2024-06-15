using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Users;

public class Character
{
    public Character(PointData pointData, CharacterInformation info, bool isEquipWeapon2)
    {
        PointData = pointData;
        Spec = CharacterSpec.GetCharacterSpec(info.Kind);
        State = CharacterState.Initialize(info, isEquipWeapon2);
        CombatStatistics = new CombatStatistics();
    }


    public static float KILLHEIGHT { get; } = -10;

    public static float WALKSPEED { get; } = 1.33f;

    public static float RUMMULTIPLIER { get; } = 2f;

    public static float GRAVITY { get; } = 9.8f;

    public static float JUMPPOWER { get; } = 3.8f;

    public static float SLIDINGFORCE { get; } = 5f;

    public static float GROUNDTHRETHOLD { get; } = 0.01f;

    public static float ARMROTATIONSPEED { get; } = 100f;

    public static float ARMLOOKATSPEED { get; } = 10f;


    public CharacterSpec Spec { get; }

    public PointData PointData { get; }

    public CharacterState State { get; private set; }

    public InputterState InputterState { get; private set; }

    public Vector3 Position { get; private set; }

    public CombatStatistics CombatStatistics { get; }

    public short ID => PointData.Data4;

    public bool IsZombie => Spec.AISkill.IsZombie();

    public float ShootingErrorHealthFactorMax => (100 - Mathf.Clamp(State.HitPoint, 0, 100)) / 100 * 1f;

    public float ShootingErrorGroundFactorMax => State.IsGround ? 0 : 3;

    public float ShootingErrorMoveFactorMax => Mathf.Abs(InputterState.Inputter.Horizontal) > 0 || Mathf.Abs(InputterState.Inputter.Vertical) > 0 ? 1.5f : 0;

    public float ShootingErrorWalkFactorMax => InputterState.Inputter.Walk ? 1f : 0;

    public float MaxShootingError
    {
        get
        {
            return ShootingErrorGroundFactorMax +
                   ShootingErrorMoveFactorMax +
                   ShootingErrorWalkFactorMax +
                   ShootingErrorHealthFactorMax +
                   State.CurrentWeapon.Spec.Accuracy.Evaluate(State.AccuracyRatio);
        }
    }

    public IObservable<Character> OnDied => _onDiedSubject;

    public IObservable<Vector3> OnDamaged => _onDamagedSubject;

    public IObservable<Vector3> OnClosed => _onClosedSubject;

    public IObservable<Vector3> OnGunSound => _onGunSoundSubject;

    public IObservable<Unit> OnChangeZoom => _onChangeZoomSubject;

    public IObservable<Unit> OnFire => _onFireSubject;

    public IObservable<BulletHitInfo> OnHit => _onHitSubject;

    public IObservable<Unit> OnWeaponVisualUpdated => _onWeaponVisualUpdatedSubject;

    public IObservable<UnityAction<BulletHitInfo>> OnGrenadeFire => _onGrenadeFireSubject;

    public IObservable<Tuple<UnityAction<BulletHitInfo>, float>> OnGunFire => _onGunFireSubject;

    public IObservable<UnityAction<BulletHitInfo>> OnZombieFire => _onZombieFireSubject;

    public IObservable<float> OnCameraMuzzleJump => _onCameraMuzzleJumpSubject;

    public IObservable<Unit> OnDropWeapon => _onDropWeaponSubject;

    public IObservable<Tuple<bool, bool, float>> OnAnimation => _onAnimationSubject;

    public void SetState(CharacterState characterState)
    {
        State = characterState;
    }

    public void SetInputterState(InputterState inputterState)
    {
        InputterState = inputterState;
    }

    public float GetShootingError()
    {
        return UnityEngine.Random.Range(0, ShootingErrorGroundFactorMax) +
               UnityEngine.Random.Range(0, ShootingErrorMoveFactorMax) +
               UnityEngine.Random.Range(0, ShootingErrorWalkFactorMax) +
               UnityEngine.Random.Range(0, ShootingErrorHealthFactorMax) +
               State.CurrentWeapon.Spec.Accuracy.Evaluate(UnityEngine.Random.Range(0, State.AccuracyRatio));
    }

    public void Loading()
    {
        _loadingSubscriber = Observable.FromCoroutine(Load).Subscribe();
    }

    public void Reloading()
    {
        _reloadingSubscriber = Observable.FromCoroutine(Reload).Subscribe();
    }

    public void ZombieLoading()
    {
        _zombieLoadingSubscriber = Observable.FromCoroutine(ZombieLoad).Subscribe();
    }

    public void Kill()
    {
        _onDiedSubject.OnNext(this);
        _onDiedSubject.OnCompleted();

        SetState(State.Kill());
    }

    public BulletHitInfo TakeBaseDamage(float baseDamage, RaycastHit hit, Vector3 direction)
    {
        BulletHitInfo.HitPartEnum hitPart = BulletHitInfo.CheckHitPart(hit);
        float damage = baseDamage * BulletHitInfo.GetDamageCoefficient(hitPart);

        return TakeDamage(damage, direction, hitPart);
    }

    public BulletHitInfo TakeDamage(float damage, Vector3 direction, BulletHitInfo.HitPartEnum hitPart = BulletHitInfo.HitPartEnum.None)
    {
        SetState(State.TakeDamage(damage));
        _onDamagedSubject.OnNext(direction);

        return new BulletHitInfo(hitPart, State.IsDied);
    }

    public void RaiseOnClose(Vector3 direction)
    {
        _onClosedSubject.OnNext(direction);
    }

    public void RaiseOnGunSound(Vector3 direction)
    {
        _onGunSoundSubject.OnNext(direction);
    }

    public void ChangeZoom()
    {
        SetState(State.SetIsZoom(!State.IsZoom));
        _onChangeZoomSubject.OnNext(Unit.Default);
    }

    public void Fire()
    {
        if (State.CurrentWeapon.Spec.IsGrenade)
        {
            _onGrenadeFireSubject.OnNext(RaiseOnHit);

            _onFireSubject.OnNext(Unit.Default);

            Loading();

            if (State.CurrentWeapon.HasAmmo)
            {
                Reloading();
            }

            if (State.CurrentWeapon.OutOfAmmo)
            {
                SetState(State.SetCurrentWeaponState(WeaponState.None));
                _onWeaponVisualUpdatedSubject.OnNext(Unit.Default);
            }
        }
        else
        {
            _onGunFireSubject.OnNext(new Tuple<UnityAction<BulletHitInfo>, float>(RaiseOnHit, State.CurrentWeapon.Spec.MuzzleJump));

            _onFireSubject.OnNext(Unit.Default);

            if (State.IsZoom)
            {
                _onCameraMuzzleJumpSubject.OnNext(State.CurrentWeapon.Spec.GetCameraMuzzleJump);
            }

            // 射撃誤差を増加
            if (State.AccuracyRatio < 1)
            {
                SetState(State.SetAccuracyRatio(Mathf.Clamp(State.AccuracyRatio + 0.2f, 0, 1)));
            }

            Loading();

            if (!State.CurrentWeapon.Spec.FullAuto)
            {
                SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsSemiFired(true)));
            }
        }
    }

    public void ZombieFire()
    {
        _onZombieFireSubject.OnNext(RaiseOnHit);

        _onFireSubject.OnNext(Unit.Default);

        ZombieLoading();
    }

    public void Update(Vector3 position, bool newIsGround, RaycastHit[] slideHits, bool isSlide, Func<Vector3, Vector3> transformDirectionFunction, Func<Vector3, CollisionFlags> MoveFunction)
    {
        Position = position;

        if (State.IsDied) Kill();
        if (Position.y < KILLHEIGHT) Kill();

        Vector3 moveDelta = State.MoveDelta;

        moveDelta.x = 0;
        moveDelta.z = 0;

        // 着地判定
        if (!State.IsGround && newIsGround)
        {
            // 落下ダメージ
            if (moveDelta.y < -8)
            {
                TakeDamage((moveDelta.y + 7) * -50, Vector3.zero);
            }

            moveDelta.y = 0;
        }

        // 地に足ついている時の処理
        if (newIsGround)
        {
            if (!isSlide && InputterState.Inputter.JumpEnter)
            {
                moveDelta.y = JUMPPOWER;
                SetState(State.SetIsGround(false));
            }
            else
            {
                moveDelta.y = -GRAVITY * Time.deltaTime;
            }
        }
        else
        {
            moveDelta.y -= GRAVITY * Time.deltaTime;
        }

        if (!State.CurrentWeapon.Spec.IsNone)
        {
            if (InputterState.Inputter.DropWeaponEnter && !State.CurrentWeapon.IsReloading && !State.IsSwitching)
            {
                _onDropWeaponSubject.OnNext(Unit.Default);
            }
            else if (State.CurrentWeapon.HasAmmo && InputterState.Inputter.ReloadEnter && !State.CurrentWeapon.IsReloading && !State.IsSwitching)
            {
                Reloading();
            }
            else if (State.CurrentWeapon.Spec.CanChangeFireMode && InputterState.Inputter.FireModeEnter)
            {
                State.CurrentWeapon.ChangeFireMode();
            }
            else
            {
                SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsTriggerPulled(InputterState.Inputter.Fire)));
            }

            if (State.CurrentWeapon.Spec.HasScope && InputterState.Inputter.ZoomEnter)
            {
                ChangeZoom();
            }
        }

        if (IsZombie)
        {
            SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsTriggerPulled(InputterState.Inputter.Fire)));
        }

        if (InputterState.Inputter.SwitchWeaponEnter && !State.CurrentWeapon.IsReloading && !State.IsSwitching)
        {
            _switchWeaponSubscriber = Observable.FromCoroutine(SwitchWeapon).Subscribe();
        }

        if (InputterState.Inputter.Walk)
        {
            var inputDelta = Vector3.forward * WALKSPEED;
            inputDelta = transformDirectionFunction(inputDelta);

            moveDelta.x += inputDelta.x;
            moveDelta.z += inputDelta.z;

            _onAnimationSubject.OnNext(new Tuple<bool, bool, float>(true, false, 1.0f));
        }
        else
        {
            var inputDelta = new Vector3(InputterState.Inputter.Horizontal, 0, InputterState.Inputter.Vertical).normalized * WALKSPEED;

            if (inputDelta.z == 0)
            {
                _onAnimationSubject.OnNext(new Tuple<bool, bool, float>(false, false, 0f));
            }
            else if (inputDelta.z > 0)
            {
                inputDelta.z *= RUMMULTIPLIER;

                _onAnimationSubject.OnNext(new Tuple<bool, bool, float>(false, true, 1.0f));
            }
            else
            {
                _onAnimationSubject.OnNext(new Tuple<bool, bool, float>(true, false, -1.0f));
            }

            inputDelta = transformDirectionFunction(inputDelta);

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

        MoveFunction(moveDelta * Time.deltaTime);
        SetState(State.SetMoveDelta(moveDelta));

        // 武器系統
        if (IsZombie)
        {
            if (State.CurrentWeapon.IsTriggerPulled && !State.CurrentWeapon.IsLoading)
            {
                ZombieFire();
            }
        }
        else
        {
            if (State.CurrentWeapon.IsTriggerPulled && !State.CurrentWeapon.IsLoading && !State.CurrentWeapon.OutOfMagazine && !State.CurrentWeapon.IsReloading && !State.CurrentWeapon.IsSemiFired)
            {
                Fire();
            }

            if (State.CurrentWeapon.IsSemiFired && !State.CurrentWeapon.IsTriggerPulled)
            {
                SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsSemiFired(false)));
            }

            SetState(State.SetAccuracyRatio(Mathf.Clamp(State.AccuracyRatio - 1f * Time.deltaTime, State.CurrentWeapon.Spec.GetCorrectedMinAccuracyRatio(State.IsZoom), 1)));
        }

        // 接地フラグを更新
        SetState(State.SetIsGround(newIsGround));
    }

    private void RaiseOnHit(BulletHitInfo bulletHitInfo)
    {
        _onHitSubject.OnNext(bulletHitInfo);
    }

    private IEnumerator Load()
    {
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.StartLoading()));
        yield return State.CurrentWeapon.Spec.WaitForFireInterval;
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsLoading(false)));

        _loadingSubscriber.Dispose();
        _loadingSubscriber = null;
    }

    private IEnumerator Reload()
    {
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsReloading(true)));
        yield return State.CurrentWeapon.Spec.WaitForReloadTime;
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.EndReloading()));

        _reloadingSubscriber.Dispose();
        _reloadingSubscriber = null;
    }

    private IEnumerator ZombieLoad()
    {
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsLoading(true)));
        yield return new WaitForSeconds(1);
        SetState(State.SetCurrentWeaponState(State.CurrentWeapon.SetIsLoading(false)));

        _zombieLoadingSubscriber.Dispose();
        _zombieLoadingSubscriber = null;
    }

    private IEnumerator SwitchWeapon()
    {
        SetState(State.ChangeSwitting(true));

        if (State.IsZoom) ChangeZoom();
        yield return new WaitForSeconds(0.2f);

        SetState(State.SwitchWeapon());

        _onWeaponVisualUpdatedSubject.OnNext(Unit.Default);

        SetState(State.ChangeSwitting(false));

        _switchWeaponSubscriber.Dispose();
        _switchWeaponSubscriber = null;
    }

    private IDisposable _loadingSubscriber;
    private IDisposable _reloadingSubscriber;
    private IDisposable _zombieLoadingSubscriber;
    private IDisposable _switchWeaponSubscriber;
    private Subject<Character> _onDiedSubject { get; } = new Subject<Character>();
    private Subject<Vector3> _onDamagedSubject { get; } = new Subject<Vector3>();
    private Subject<Vector3> _onClosedSubject { get; } = new Subject<Vector3>();
    private Subject<Vector3> _onGunSoundSubject { get; } = new Subject<Vector3>();
    private Subject<Unit> _onChangeZoomSubject { get; } = new Subject<Unit>();
    private Subject<Unit> _onFireSubject { get; } = new Subject<Unit>();
    private Subject<BulletHitInfo> _onHitSubject { get; } = new Subject<BulletHitInfo>();

    private Subject<Unit> _onWeaponVisualUpdatedSubject { get; } = new Subject<Unit>();

    private Subject<UnityAction<BulletHitInfo>> _onGrenadeFireSubject { get; } = new Subject<UnityAction<BulletHitInfo>>();

    private Subject<Tuple<UnityAction<BulletHitInfo>, float>> _onGunFireSubject { get; } = new Subject<Tuple<UnityAction<BulletHitInfo>, float>>();

    private Subject<UnityAction<BulletHitInfo>> _onZombieFireSubject { get; } = new Subject<UnityAction<BulletHitInfo>>();

    private Subject<float> _onCameraMuzzleJumpSubject { get; } = new Subject<float>();

    private Subject<Unit> _onDropWeaponSubject { get; } = new Subject<Unit>();

    private Subject<Tuple<bool, bool, float>> _onAnimationSubject = new Subject<Tuple<bool, bool, float>>();
}
