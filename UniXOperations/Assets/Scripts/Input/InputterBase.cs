using UniRx;
using UnityEngine;

public class InputterBase
{
    #region Property

    public float Horizontal { get; protected set; }

    public float Vertical { get; protected set; }

    public bool Fire
    {
        get { return _fire.Value; }
        protected set { _fire.Value = value; }
    }

    public bool Jump
    {
        get { return _jump.Value; }
        protected set { _jump.Value = value; }
    }

    public float MouseX { get; protected set; }

    public float MouseY { get; protected set; }

    public bool Walk { get; protected set; }

    public bool Reload
    {
        get { return _reload.Value; }
        protected set { _reload.Value = value; }
    }

    public bool DropWeapon
    {
        get { return _dropWeapon.Value; }
        protected set { _dropWeapon.Value = value; }
    }

    public bool Zoom
    {
        get { return _zoom.Value; }
        protected set { _zoom.Value = value; }
    }

    public bool FireMode
    {
        get { return _fireMode.Value; }
        protected set { _fireMode.Value = value; }
    }

    public bool SwitchWeapon
    {
        get { return _switchWeapon.Value; }
        protected set { _switchWeapon.Value = value; }
    }

    public bool Weapon1
    {
        get { return _weapon1.Value; }
        protected set { _weapon1.Value = value; }
    }

    public bool Weapon2
    {
        get { return _weapon2.Value; }
        protected set { _weapon2.Value = value; }
    }


    public bool FireEnter { get; private set; }

    public bool JumpEnter { get; private set; }

    public bool ReloadEnter { get; private set; }

    public bool DropWeaponEnter { get; private set; }

    public bool ZoomEnter { get; private set; }

    public bool FireModeEnter { get; private set; }

    public bool SwitchWeaponEnter { get; private set; }

    public bool Weapon1Enter { get; private set; }

    public bool Weapon2Enter { get; private set; }

    #endregion

    public InputterBase(GameObject gameObject)
    {
        Observable.EveryUpdate().WithLatestFrom(_fire, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => FireEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_jump, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => JumpEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_reload, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => ReloadEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_dropWeapon, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => DropWeaponEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_zoom, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => ZoomEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_fireMode, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => FireModeEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_switchWeapon, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => SwitchWeaponEnter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_weapon1, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => Weapon1Enter = v).AddTo(gameObject);
        Observable.EveryUpdate().WithLatestFrom(_weapon2, (s1, s2) => s2).Pairwise((v1, v2) => !v1 && v2).Subscribe(v => Weapon2Enter = v).AddTo(gameObject);

        Observable.EveryUpdate().Do(_ => InputUpdate()).Subscribe().AddTo(gameObject);
    }

    #region Method

    protected virtual void InputUpdate()
    {

    }

    #endregion

    #region Field

    private readonly BoolReactiveProperty _fire = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _jump = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _reload = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _dropWeapon = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _zoom = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _fireMode = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _switchWeapon = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _weapon1 = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _weapon2 = new BoolReactiveProperty();

    #endregion
}
