public class AIInputter : InputterBase
{
    public AIInputter(Character characterState, PathContainer firstPath, GameDataContainer gameDataContainer, short aISkill) : base(characterState.gameObject)
    {
        _aIBehaviorData = new AIBehaviorData(
            new AIController(),
            characterState,
            firstPath,
            gameDataContainer,
            AISkill.GetAISkill(aISkill));

        _aIStateManager = new AIStateManager(_aIBehaviorData);
    }

    protected override void InputUpdate()
    {
        _aIStateManager.Update();

        Horizontal = _aIBehaviorData.Controller.Horizontal;
        Vertical = _aIBehaviorData.Controller.Vertical;
        Fire = _aIBehaviorData.Controller.Fire;
        Jump = _aIBehaviorData.Controller.Jump;
        MouseX = _aIBehaviorData.Controller.MouseX;
        MouseY = _aIBehaviorData.Controller.MouseY;
        Walk = _aIBehaviorData.Controller.Walk;
        Reload = _aIBehaviorData.Controller.Reload;
        DropWeapon = _aIBehaviorData.Controller.DropWeapon;
        Zoom = _aIBehaviorData.Controller.Zoom;
        FireMode = _aIBehaviorData.Controller.FireMode;
        SwitchWeapon = _aIBehaviorData.Controller.SwitchWeapon;
        Weapon1 = _aIBehaviorData.Controller.Weapon1;
        Weapon2 = _aIBehaviorData.Controller.Weapon2;
    }

    #region PrivateField

    private AIBehaviorData _aIBehaviorData;
    private AIStateManager _aIStateManager;

    #endregion
}
