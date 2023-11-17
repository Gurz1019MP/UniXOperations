using UnityEngine;
using UniRx;

public class CombatStatistics
{
    public int RoundsFired { get; set; }

    public int RoundsOnTarget { get; set; }

    public float AccuracyRate
    {
        get
        {
            if (RoundsFired == 0) return 0;

            return (float)RoundsOnTarget * 100 / RoundsFired;
        }
    }

    public int Kill { get; set; }

    public int HeadShot { get; set; }

    public void OnFire(Unit _)
    {
        RoundsFired++;
        //Debug.Log($"Fire : {RoundsFired}");
    }

    public void OnHit(BulletHitInfo bulletHitInfo)
    {
        RoundsOnTarget++;
        if (bulletHitInfo.IsKill) Kill++;
        if (bulletHitInfo.HitPart == BulletHitInfo.HitPartEnum.Head) HeadShot++;

        //Debug.Log($"Hit : {RoundsOnTarget}, Kill : {Kill}, HS : {HeadShot}");
    }
}
