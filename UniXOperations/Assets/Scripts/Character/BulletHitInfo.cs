public struct BulletHitInfo
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
}
