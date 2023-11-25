using System.Collections.Generic;
using UniRx;

[System.Serializable]
public class AIStateManager
{
    public AbstractAIBehavior CurrentStateAI { get; protected set; }

    private CharacterState _characterState;
    private CommonAI _commonAI;
    private Dictionary<StateKind, AbstractAIBehavior> _stateAIMapper;

    public AIStateManager(IAIBehaviorData aIComponentData)
    {
        _characterState = aIComponentData.CharacterState;
        _commonAI = new CommonAI(aIComponentData);
        _stateAIMapper = new Dictionary<StateKind, AbstractAIBehavior>()
        {
            { StateKind.Safe, aIComponentData.AISkill.SafeAICreator(aIComponentData) },
            { StateKind.Alert, aIComponentData.AISkill.AlertAICreator(aIComponentData) },
            { StateKind.Combat, aIComponentData.AISkill.CombatAICreator(aIComponentData) },
        };

        CurrentStateAI = _stateAIMapper[StateKind.Safe];
        CurrentStateAI.EnterState();

        _commonAI.StateMode.Pairwise((v1, v2) =>
        {
            _stateAIMapper[v1].LeaveState();
            _stateAIMapper[v2].EnterState();

            if (v2 == StateKind.Alert)
            {
                (_stateAIMapper[StateKind.Safe] as PathAI)?.SetWasAlert();
            }

            return v2;
        }).Subscribe(s =>
        {
            CurrentStateAI = _stateAIMapper[s];
        }).AddTo(_characterState);

        _commonAI.TargetEnemy.Subscribe(e =>
        {
            (_stateAIMapper[StateKind.Combat] as ICombatAI).SetTargetEnemy(_commonAI.TargetEnemy.Value);
        }).AddTo(_characterState);

        _commonAI.AlertDirection.Subscribe(e =>
        {
            (_stateAIMapper[StateKind.Alert] as AlertAI)?.SetAttackedDirection(e);
        }).AddTo(_characterState);
    }

    #region PublicMethod

    public void Update()
    {
        CurrentStateAI.Update();

#if UNITY_EDITOR
        _characterState.DebugText.text = $"State : {_commonAI.StateMode}\r\nHP : {_characterState.HitPoint}";
#else
#endif
    }

    #endregion

    #region Type

    public enum StateKind
    {
        Safe,
        Alert,
        Combat,
    }

    #endregion
}
