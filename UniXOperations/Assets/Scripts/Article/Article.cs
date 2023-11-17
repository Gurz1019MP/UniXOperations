using System;
using UniRx;
using UnityEngine;

[Serializable]
public class Article
{
    [SerializeField, ReadOnly]
    public short ID;

    [SerializeField, ReadOnly]
    public float HitPoint;

    public IObservable<Unit> OnDestroyed => _onDestroyedSubject;

    public Article(short id, ArticleSpec spec)
    {
        // パラメータの設定
        _spec = spec;
        ID = id;
        HitPoint = _spec.HitPoint;
    }

    public void TakeDamage(float baseDamage)
    {
        HitPoint -= baseDamage;
        if (HitPoint < 0)
        {
            _onDestroyedSubject.OnNext(Unit.Default);
            _onDestroyedSubject.OnCompleted();
        }

        // Debug.Log($"Article TakeDamage (Id:{ID}, HitPoint:{HitPoint})");
    }

    private ArticleSpec _spec;
    private readonly Subject<Unit> _onDestroyedSubject = new Subject<Unit>();
}
