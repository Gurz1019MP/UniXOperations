using System;
using UnityEngine.Events;

public interface IBullet
{
    float Damage { get; set; }
    float Penetration { get; set; }
    float Speed { get; set; }
    short Team { get; set; }
    float Sound { get; set; }

    IObservable<Tuple<IBullet, BulletHitInfo>> OnHit { get; }
}
