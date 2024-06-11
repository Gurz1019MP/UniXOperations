using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class AISkill
{
    #region .ctor

    private AISkill(short id, 
                    float propControlConstant, 
                    float detectionRange, 
                    float detectionAngleHorizontal, 
                    float detectionAngleVertical,
                    float detectionDelay, 
                    float runningDetectionRange, 
                    float missDelay, 
                    float alertTime, 
                    float aimHeightOffset, 
                    float shootThreshold, 
                    float shootError)
    {
        this.id = id;
        this.propControlConstant = propControlConstant;
        this.detectionRange = detectionRange;
        this.detectionAngleHorizontal = detectionAngleHorizontal;
        this.detectionAngleVertical = detectionAngleVertical;
        this.detectionDelay = detectionDelay;
        this.runningDetectionRange = runningDetectionRange;
        this.missDelay = missDelay;
        this.alertTime = alertTime;
        this.aimHeightOffset = aimHeightOffset;
        this.shootThreshold = shootThreshold;
        this.shootError = shootError;
    }

    #endregion

    #region PublicMethod

    public static AISkill GetAISkill(short skill)
    {
        try
        {
            AISkill result = Skills.Single(s => s.id == skill);

            return result;
        }
        catch(Exception ex)
        {
            Debug.LogError($"AISkillの読み込みで定義されていないIdを読み込みました(Id:{skill}){ex}");
            return null;
        }
    }

    public bool IsZombie()
    {
        return id == -1;
    }

    public float GetCorrectionOperatingVolume(float volume)
    {
        return volume * propControlConstant;
    }

    public bool IsInRange(float distance, float directionAngleHorizontal, float directionAngleVertical)
    {
        return distance < detectionRange &&
               directionAngleHorizontal < detectionAngleHorizontal &&
               directionAngleVertical < detectionAngleVertical;
    }

    public bool IsInRunningRange(float distance)
    {
        return distance < runningDetectionRange;
    }

    public float GetDistanceScale(float distance)
    {
        return distance / detectionRange;
    }

    public float GetError(float distance)
    {
        return shootError * GetDistanceScale(distance);
    }

    public Vector3 AimHeightOffsetVector(Vector3 up)
    {
        return up * aimHeightOffset;
    }

    public bool BeShoot(float orientationErrorHorizontal, float orientationErrorVertical)
    {
        return Mathf.Abs(orientationErrorHorizontal) < shootThreshold && Mathf.Abs(orientationErrorVertical) < shootThreshold;
    }

    public WaitForSeconds WaitForDetectionDelay()
    {
        return new WaitForSeconds(detectionDelay);
    }

    public WaitForSeconds WaitForMiss()
    {
        return new WaitForSeconds(missDelay * UnityEngine.Random.Range(0.5f, 1.5f));
    }

    public WaitForSeconds WaitForAlertToSafe()
    {
        return new WaitForSeconds(alertTime * UnityEngine.Random.Range(0.8f, 1.5f));
    }

    #endregion

    #region PrivateField

    private static readonly AISkill[] Skills = new AISkill[]
    {
        new(1, 0.02f, 20f, 50f, 30f, 0.5f, 2f, 3f, 5f, 0f, 20f, 6f),
        new(2, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0f, 10f, 4.5f),
        new(3, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 10f, 3f),
        new(4, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 10f, 1.5f),
        new(5, 0.03f, 30f, 80f, 30f, 0.5f, 3f, 3f, 5f, 0.5f, 0.5f, 0f),
        new(-1, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 0.5f, 0f),
    };

    private readonly short id;
    private readonly float propControlConstant;
    private readonly float detectionRange;
    private readonly float detectionAngleHorizontal;
    private readonly float detectionAngleVertical;
    private readonly float detectionDelay;
    private readonly float runningDetectionRange;
    private readonly float missDelay;
    private readonly float alertTime;
    private readonly float aimHeightOffset;
    private readonly float shootThreshold;
    private readonly float shootError;

    #endregion
}
