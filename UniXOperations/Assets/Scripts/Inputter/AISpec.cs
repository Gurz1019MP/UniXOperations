using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct AISpec : IEquatable<AISpec>
{
    #region .ctor

    static AISpec()
    {
        _Empty = new AISpec(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true);
    }

    private AISpec(short id, 
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
                    float shootError,
                    bool isEmpty = false)
    {
        this._id = id;
        this._propControlConstant = propControlConstant;
        this._detectionRange = detectionRange;
        this._detectionAngleHorizontal = detectionAngleHorizontal;
        this._detectionAngleVertical = detectionAngleVertical;
        this._detectionDelay = detectionDelay;
        this._runningDetectionRange = runningDetectionRange;
        this._missDelay = missDelay;
        this._alertTime = alertTime;
        this._aimHeightOffset = aimHeightOffset;
        this._shootThreshold = shootThreshold;
        this._shootError = shootError;
        this._isEmpty = isEmpty;
    }

    #endregion

    #region PublicMethod

    public static AISpec GetAISkill(short skill)
    {
        try
        {
            AISpec result = Skills.Single(s => s._id == skill);

            return result;
        }
        catch(Exception ex)
        {
            Debug.LogError($"AISkillの読み込みで定義されていないIdを読み込みました(Id:{skill}){ex}");
            return _Empty;
        }
    }

    public bool IsZombie()
    {
        return _id == -1;
    }

    public float GetCorrectionOperatingVolume(float volume)
    {
        return volume * _propControlConstant;
    }

    public bool IsInRange(float distance, float directionAngleHorizontal, float directionAngleVertical)
    {
        return distance < _detectionRange &&
               directionAngleHorizontal < _detectionAngleHorizontal &&
               directionAngleVertical < _detectionAngleVertical;
    }

    public bool IsInRunningRange(float distance)
    {
        return distance < _runningDetectionRange;
    }

    public float GetDistanceScale(float distance)
    {
        return distance / _detectionRange;
    }

    public float GetError(float distance)
    {
        return _shootError * GetDistanceScale(distance);
    }

    public Vector3 AimHeightOffsetVector(Vector3 up)
    {
        return up * _aimHeightOffset;
    }

    public bool BeShoot(float orientationErrorHorizontal, float orientationErrorVertical)
    {
        return Mathf.Abs(orientationErrorHorizontal) < _shootThreshold && Mathf.Abs(orientationErrorVertical) < _shootThreshold;
    }

    public WaitForSeconds WaitForDetectionDelay()
    {
        return new WaitForSeconds(_detectionDelay);
    }

    public WaitForSeconds WaitForMiss()
    {
        return new WaitForSeconds(_missDelay * UnityEngine.Random.Range(0.5f, 1.5f));
    }

    public WaitForSeconds WaitForAlertToSafe()
    {
        return new WaitForSeconds(_alertTime * UnityEngine.Random.Range(0.8f, 1.5f));
    }

    #region IEquatable

    public override bool Equals(object obj) => obj is AISpec other && this.Equals(other);

    public bool Equals(AISpec p)
    {
        return _id == p._id &&
               _propControlConstant == p._propControlConstant &&
               _detectionRange == p._detectionRange &&
               _detectionAngleHorizontal == p._detectionAngleHorizontal &&
               _detectionAngleVertical == p._detectionAngleVertical &&
               _detectionDelay == p._detectionDelay &&
               _runningDetectionRange == p._runningDetectionRange &&
               _missDelay == p._missDelay &&
               _alertTime == p._alertTime &&
               _aimHeightOffset == p._aimHeightOffset &&
               _shootThreshold == p._shootThreshold &&
               _shootError == p._shootError &&
               _isEmpty == p._isEmpty;
    } 

    public override int GetHashCode() => (_id, 
                                          _propControlConstant, 
                                          _detectionRange, 
                                          _detectionAngleHorizontal, 
                                          _detectionAngleVertical, 
                                          _detectionDelay,
                                          _runningDetectionRange, 
                                          _missDelay, 
                                          _alertTime, 
                                          _aimHeightOffset, 
                                          _shootThreshold, 
                                          _shootError, 
                                          _isEmpty).GetHashCode();

    public static bool operator ==(AISpec lhs, AISpec rhs) => lhs.Equals(rhs);

    public static bool operator !=(AISpec lhs, AISpec rhs) => !(lhs == rhs);

    #endregion

    #endregion

    #region Field

    private static readonly AISpec[] Skills = new AISpec[]
    {
        new(1, 0.02f, 20f, 50f, 30f, 0.5f, 2f, 3f, 5f, 0f, 20f, 6f),
        new(2, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0f, 10f, 4.5f),
        new(3, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 10f, 3f),
        new(4, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 10f, 1.5f),
        new(5, 0.03f, 30f, 80f, 30f, 0.5f, 3f, 3f, 5f, 0.5f, 0.5f, 0f),
        new(-1, 0.02f, 20f, 60f, 30f, 0.5f, 2f, 3f, 5f, 0.5f, 0.5f, 0f),
    };

    private static readonly AISpec _Empty;

    private readonly short _id;
    private readonly float _propControlConstant;
    private readonly float _detectionRange;
    private readonly float _detectionAngleHorizontal;
    private readonly float _detectionAngleVertical;
    private readonly float _detectionDelay;
    private readonly float _runningDetectionRange;
    private readonly float _missDelay;
    private readonly float _alertTime;
    private readonly float _aimHeightOffset;
    private readonly float _shootThreshold;
    private readonly float _shootError;
    private readonly bool _isEmpty;

    #endregion
}
