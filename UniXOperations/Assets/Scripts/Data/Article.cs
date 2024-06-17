using System;
using UniRx;

public class Article
{
    public Article(PointData pointData)
    {
        Id = pointData.Data4;
        PointData = pointData;
        Spec = ArticleSpec.GetArticleSpec(pointData.Data2);
    }

    public short Id { get; }

    public PointData PointData { get; }

    public ArticleSpec Spec { get; }

    public ArticleState State { get; private set; }

    public bool IsGroundFlag => PointData.Data3 == 0;

    public IObservable<Article> OnDestroyed => _onDestroyedSubject;

    public void TakeDamage(float damage)
    {
        SetState(State.BuildWithHitpoint(damage));
    }

    public void SetState(ArticleState state)
    {
        State = state;
        if (State.IsDestroyed)
        {
            _onDestroyedSubject.OnNext(this);
            _onDestroyedSubject.OnCompleted();
        }
    }

    private readonly Subject<Article> _onDestroyedSubject = new Subject<Article>();
}
