using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class Grenade : MonoBehaviour, IBullet
{
    public float Damage { get; set; }
    public float Penetration { get; set; }
    public float Speed { get; set; }
    public Team Team { get; set; } = Team.Empty;
    public float Sound { get; set; }

    public LayerMask HitMask;
    public float BlastRadius;

    public float _detonationTimer;

    public GameObject BombEffect;

    private Subject<Tuple<IBullet, BulletHitInfo>> _onHitSubject = new Subject<Tuple<IBullet, BulletHitInfo>>();
    public IObservable<Tuple<IBullet, BulletHitInfo>> OnHit => _onHitSubject;

    private void Start()
    {
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.AddForce(transform.forward * Speed, ForceMode.VelocityChange);

        StartCoroutine(DetonationTimer());
    }

    private IEnumerator DetonationTimer()
    {
        yield return new WaitForSeconds(_detonationTimer);
        Detonation();
    }

    private void Detonation()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, BlastRadius, transform.forward, 0, HitMask);
        if (hits.Any())
        {
            foreach (RaycastHit hit in hits)
            {
                //Debug.Log($"hit {hit.collider.gameObject.name}");
                var distance = (hit.transform.position - transform.position).magnitude;
                var characterContainer = hit.collider.gameObject.GetComponentInParent<CharacterContainer>();
                var articleContainer = hit.collider.gameObject.GetComponentInParent<ArticleContainer>();

                if (characterContainer != null)
                {
                    if (!Physics.Linecast(transform.position, characterContainer.transform.position, LayerMask.GetMask("Stage")))
                    {
                        var damage = (BlastRadius - distance) / BlastRadius * Damage;
                        Debug.Log($"Character Hit, distance : {distance}, damage : {damage}");
                        var direction = (characterContainer.transform.position - transform.position).normalized;
                        var bulletHitInfo = characterContainer.Character.TakeDamage(damage, direction);
                        characterContainer.KnockBack(direction, (damage / Damage) * 0.5f);

                        _onHitSubject.OnNext(new Tuple<IBullet, BulletHitInfo>(this, bulletHitInfo));
                        _onHitSubject.OnCompleted();
                    }
                }
                else if (articleContainer != null)
                {
                    if (!Physics.Linecast(transform.position, articleContainer.transform.position, LayerMask.GetMask("Stage")))
                    {
                        var damage = (BlastRadius - distance) / BlastRadius * Damage;
                        Debug.Log($"Article Hit, distance : {distance}, damage : {damage}");
                        articleContainer.Article.State.TakeDamage(damage);
                    }
                }
            }
        }

        Destroy(gameObject);

        Instantiate(BombEffect, transform.position, Quaternion.identity);
    }
}
