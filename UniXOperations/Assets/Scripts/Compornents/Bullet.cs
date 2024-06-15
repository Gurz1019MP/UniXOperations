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
    public Team Team { get; set; } = Team.Empty;
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

        _bulletMoveDelta = Speed * Time.deltaTime * transform.forward;
        
        RaycastHit[] sounds = Physics.SphereCastAll(transform.position, Sound, Vector3.up, 0f, LayerMask.GetMask(ConstantsManager.LayerMask_Character_Root));
        if (sounds.Any())
        {
            foreach (CharacterContainer characterContainer in sounds.Select(c => c.collider.gameObject.GetComponent<CharacterContainer>()).Where(cs => cs != null).Where(cs => cs.Character.State.Team == Team))
            {
                characterContainer.Character.RaiseOnGunSound(characterContainer.transform.position - transform.position);
                //Debug.Log("Sound");
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
            foreach (CharacterContainer characterContainer in closes.Select(c => c.collider.gameObject.GetComponent<CharacterContainer>()).Where(cs => cs != null))
            {
                characterContainer.Character.RaiseOnClose(displacementSinceLastFrame.normalized);
            }
        }

        RaycastHit[] hits = Physics.SphereCastAll(_lastPosition, 0.01f, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, Mask);
        if (hits.Any())
        {
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance))
            {
                //Debug.Log($"hit {hit.collider.gameObject.name}");
                var characterContainer = hit.collider.gameObject.GetComponentInParent<CharacterContainer>();
                var articleContainer = hit.collider.gameObject.GetComponentInParent<ArticleContainer>();

                if (characterContainer == null && articleContainer == null)
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
                if (characterContainer != null && characterContainer.Character.State.Team != Team)
                {
                    // キャラクターに命中した場合
                    var bulletHitInfo = characterContainer.Character.TakeBaseDamage(Damage, hit, displacementSinceLastFrame.normalized);
                    characterContainer.KnockBack(_bulletMoveDelta.normalized, 0.1f);

                    _onHitSubject.OnNext(new Tuple<IBullet, BulletHitInfo>(this, bulletHitInfo));
                    _onHitSubject.OnCompleted();

                    Destroy(gameObject);
                }
                else if (articleContainer != null)
                {
                    articleContainer.Article.State.TakeDamage(Damage);
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
