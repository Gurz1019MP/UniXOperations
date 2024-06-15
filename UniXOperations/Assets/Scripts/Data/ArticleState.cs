using UnityEngine;

public readonly struct ArticleState
{
    public ArticleState(float hitPoint)
    {
        _hitPoint = hitPoint;
    }

    public bool IsDestroyed => _hitPoint <= 0;

    public static ArticleState Initialize(ArticleSpec spec)
    {
        return new ArticleState(spec.HitPoint);
    }

    public readonly ArticleState TakeDamage(float baseDamage)
    {
        return new ArticleState(Mathf.Clamp(_hitPoint - baseDamage, 0, _hitPoint));
    }

    private readonly float _hitPoint;
}
