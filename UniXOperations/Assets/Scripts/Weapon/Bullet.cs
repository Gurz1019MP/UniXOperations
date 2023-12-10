using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class Bullet : MonoBehaviour, IBullet
{
    public float Damage { get; set; }
    public float Penetration { get; set; }
    public float Speed { get; set; }
    public short Team { get; set; } = -1;
    public float Sound { get; set; }
    public LayerMask Mask;

    public GameObject BulletHole;

    private Subject<Tuple<IBullet, BulletHitInfo>> _onHitSubject = new Subject<Tuple<IBullet, BulletHitInfo>>();
    public IObservable<Tuple<IBullet, BulletHitInfo>> OnHit => _onHitSubject;

    private Vector3 _bulletMoveDelta;
    private Vector3 _lastPosition;
    private float _maxLifeTime = 1f;

    void Start()
    {
        Destroy(gameObject, _maxLifeTime);

        _bulletMoveDelta = transform.forward * Speed * Time.deltaTime;
        
        RaycastHit[] sounds = Physics.SphereCastAll(transform.position, Sound, Vector3.up, 0f, LayerMask.GetMask("Character_Root"));
        if (sounds.Any())
        {
            foreach (Character characterState in sounds.Select(c => c.collider.gameObject.GetComponent<Character>()).Where(cs => cs != null).Where(cs => cs.Team != Team))
            {
                characterState.RaiseOnGunSound(characterState.transform.position - transform.position);
                Debug.Log("Sound");
            }
        }
    }

    void Update()
    {
        _lastPosition = transform.position;
        transform.position += _bulletMoveDelta;
        Debug.DrawLine(_lastPosition, transform.position, Color.red, 5f);

        Vector3 displacementSinceLastFrame = transform.position - _lastPosition;

        RaycastHit[] closes = Physics.SphereCastAll(_lastPosition, 0.3f, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, LayerMask.GetMask("Character_Root"));
        if (closes.Any())
        {
            foreach (Character characterState in closes.Select(c => c.collider.gameObject.GetComponent<Character>()).Where(cs => cs != null))
            {
                characterState.RaiseOnClose(displacementSinceLastFrame.normalized);
            }
        }

        RaycastHit[] hits = Physics.SphereCastAll(_lastPosition, 0.01f, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, Mask);
        if (hits.Any())
        {
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance))
            {
                //Debug.Log($"hit {hit.collider.gameObject.name}");
                var characterState = hit.collider.gameObject.GetComponentInParent<Character>();
                var articleState = hit.collider.gameObject.GetComponentInParent<ArticleContainer>();

                if (characterState == null && articleState == null)
                {
                    // ステージに命中した場合
                    Vector3 penetrationEndPoint = hit.point + displacementSinceLastFrame.normalized * Penetration;
                    //DrawDebugCross(penetrationEndPoint, 1F, Color.yellow, 5F);
                    if (Physics.Linecast(penetrationEndPoint, hit.point, 1 << 16))
                    {
                        //Debug.Log("貫通します");
                        Damage *= 0.4f;
                    }
                    else
                    {
                        //Debug.Log("貫通しません");
                        Destroy(gameObject);
                    }

                    Instantiate(BulletHole, hit.point, Quaternion.FromToRotation(BulletHole.transform.up * -1f, hit.normal));
                }
                if (characterState != null && characterState.Team != Team)
                {
                    // キャラクターに命中した場合
                    var bulletHitInfo = characterState.TakeDamage(Damage, hit, displacementSinceLastFrame.normalized);
                    characterState.KnockBack(_bulletMoveDelta.normalized, 0.1f);

                    _onHitSubject.OnNext(new Tuple<IBullet, BulletHitInfo>(this, bulletHitInfo));
                    _onHitSubject.OnCompleted();

                    Destroy(gameObject);
                }
                else if (articleState != null)
                {
                    articleState.Article.TakeDamage(Damage);
                }
            }
        }
    }

    private void DrawDebugCross(Vector3 point, float scale, Color color, float duration)
    {
        Debug.DrawLine(point + Vector3.up * scale, point + Vector3.up * -scale, color, duration);
        Debug.DrawLine(point + Vector3.right * scale, point + Vector3.right * -scale, color, duration);
        Debug.DrawLine(point + Vector3.forward * scale, point + Vector3.forward * -scale, color, duration);
    }
}
