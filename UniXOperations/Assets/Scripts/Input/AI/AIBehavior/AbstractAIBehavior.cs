using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public abstract class AbstractAIBehavior
{
    #region ctor

    public AbstractAIBehavior(IAIBehaviorData aIComponentData)
    {
        Controller = aIComponentData.Controller;
        CharacterState = aIComponentData.CharacterState;
        AIParameter = aIComponentData.AISkill;
        GameDataContainer = aIComponentData.GameDataContainer;
    }

    #endregion

    #region Property

    public AIController Controller { get; }

    public Character CharacterState { get; }

    public AISkill AIParameter { get; }

    public GameDataContainer GameDataContainer { get; }

    #endregion

    #region Method

    public virtual void Update() { }

    public virtual void EnterState() { }

    public virtual void LeaveState() { }

    protected IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(0.2f);
        boolSetAction(false);
    }

    protected void Stop()
    {
        Controller.Horizontal = 0;
        Controller.Vertical = 0;
        Controller.Fire = false;
        Controller.Jump = false;
        Controller.MouseX = 0;
        Controller.MouseY = 0;
        Controller.Walk = false;
        Controller.Reload = false;
        Controller.DropWeapon = false;
        Controller.Zoom = false;
        Controller.FireMode = false;
        Controller.SwitchWeapon = false;
        Controller.Weapon1 = false;
        Controller.Weapon2 = false;
    }

    #endregion
}
