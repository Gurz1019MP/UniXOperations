using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class AISkill
{
    #region Property

    public short Id;
    public float PropControlConstant;
    public float DetectionRange;
    public float DetectionAngleHorizontal;
    public float DetectionAngleVertical;
    public float DetectionDelay;
    public float RunningDetectionRange;
    public float MissDelay;
    public float AlertTime;
    public float AimHeightOffset;
    public float ShootThreshold;
    public float ShootError;

    public Func<IAIBehaviorData, AbstractAIBehavior> SafeAICreator;

    public Func<IAIBehaviorData, AbstractAIBehavior> AlertAICreator;

    public Func<IAIBehaviorData, AbstractAIBehavior> CombatAICreator;

    #endregion

    #region PublicMethod

    public static AISkill GetAISkill(short skill)
    {
        try
        {
            AISkill result = JsonContainer.Instance.AISkillArray.Single(s => s.Id == skill);
            result.SafeAICreator = Skills.SingleOrDefault(s => s.Id == result.Id)?.SafeAICreator;
            result.AlertAICreator = Skills.SingleOrDefault(s => s.Id == result.Id)?.AlertAICreator;
            result.CombatAICreator = Skills.SingleOrDefault(s => s.Id == result.Id)?.CombatAICreator;

            return result;
        }
        catch(Exception ex)
        {
            Debug.LogError($"AISkillの読み込みで定義されていないIdを読み込みました(Id:{skill}){ex}");
            return null;
        }
    }

    #endregion

    #region PrivateField

    private static AISkill[] Skills = new AISkill[]
    {
        new AISkill() { Id = 1, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 50f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0f, ShootThreshold = 20f, ShootError = 3f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        new AISkill() { Id = 2, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0f, ShootThreshold = 10f, ShootError = 1.5f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        new AISkill() { Id = 3, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 10f, ShootError = 1f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        new AISkill() { Id = 4, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 10f, ShootError = 0.75f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        new AISkill() { Id = 5, PropControlConstant = 0.03f, DetectionRange = 30f, DetectionAngleHorizontal = 80f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 3f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 0.5f, ShootError = 0.1f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        new AISkill() { Id = -1, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 0.5f, ShootError = 0f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new ZombieCombatAI(c) },
    };

    #endregion
}
