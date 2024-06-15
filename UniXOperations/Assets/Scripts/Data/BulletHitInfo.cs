using UnityEngine;

public readonly struct BulletHitInfo
{
    public BulletHitInfo(HitPartEnum hitPart, bool isKill)
    {
        HitPart = hitPart;
        IsKill = isKill;
    }

    public HitPartEnum HitPart { get; }

    public bool IsKill { get; }

    public enum HitPartEnum
    {
        None,
        Body,
        Head,
        Leg,
        Arm
    }

    public static HitPartEnum CheckHitPart(RaycastHit hit)
    {
        if (hit.collider.CompareTag(ConstantsManager.TagUp))
        {
            if (hit.point.y - hit.transform.position.y > 1.0f)
            {
                //Debug.Log("Head shot");
                return HitPartEnum.Head;
            }
            else
            {
                return HitPartEnum.Body;
            }
        }
        else if (hit.collider.CompareTag(ConstantsManager.TagLeg))
        {
            return HitPartEnum.Leg;
        }
        else if (hit.collider.CompareTag(ConstantsManager.TagArmHolding))
        {
            return HitPartEnum.Arm;
        }
        else
        {
            return HitPartEnum.None;
        }
    }

    public static float GetDamageCoefficient(HitPartEnum hitPart)
    {
        switch (hitPart)
        {
            case HitPartEnum.Body:
                return 1;
            case HitPartEnum.Head:
                return 2;
            case HitPartEnum.Leg:
                return 0.6f;
            case HitPartEnum.Arm:
                return 0.4f;
            case HitPartEnum.None:
            default:
                return 1;
        }
    }
}
