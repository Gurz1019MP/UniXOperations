using System;
using UniRx;

public class ArticleState
{
    public ArticleState(short id,
                        ArticleSpec spec)
    {
        Id = id;
        _spec = spec;
        _hitPoint = _spec.HitPoint;
    }

    public short Id { get; }
    public IObservable<Unit> OnDestroyed => _onDestroyedSubject;

    public void TakeDamage(float baseDamage)
    {
        _hitPoint -= baseDamage;
        if (_hitPoint < 0)
        {
            _onDestroyedSubject.OnNext(Unit.Default);
            _onDestroyedSubject.OnCompleted();
        }

        // Debug.Log($"Article TakeDamage (Id:{ID}, HitPoint:{HitPoint})");
    }

    private float _hitPoint;
    private readonly ArticleSpec _spec;
    private readonly Subject<Unit> _onDestroyedSubject = new Subject<Unit>();
}
