using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class ZombieAttack : MonoBehaviour, IBullet
{
    public float Damage { get; set; }

    public float Penetration { get; set; }

    public float Speed { get; set; }

    public short Team { get; set; }
    public float Sound { get; set; }
    public LayerMask Mask;
    public float Range;

    private Subject<Tuple<IBullet, BulletHitInfo>> _onHitSubject = new Subject<Tuple<IBullet, BulletHitInfo>>();
    public IObservable<Tuple<IBullet, BulletHitInfo>> OnHit => _onHitSubject;

    void Start()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, Range, transform.forward, 0, Mask);
        if (hits.Any())
        {
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance))
            {
                //Debug.Log($"hit {hit.collider.gameObject.name}");
                var characterState = hit.collider.gameObject.GetComponentInParent<CharacterState>();
                var articleState = hit.collider.gameObject.GetComponentInParent<ArticleContainer>();

                if (characterState != null && characterState.Team != Team)
                {
                    // キャラクターに命中した場合
                    var bulletHitInfo = characterState.TakeDamage(Damage, hit, transform.forward);

                    _onHitSubject.OnNext(new Tuple<IBullet, BulletHitInfo>(this, bulletHitInfo));
                    _onHitSubject.OnCompleted();

                    Debug.Log("Hit");
                }
            }
        }

        Destroy(gameObject);
    }
}
