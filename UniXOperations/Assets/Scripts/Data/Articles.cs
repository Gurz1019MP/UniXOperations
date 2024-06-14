using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Articles
{
    public Articles(ArticleContainer[] articles)
    {
        _articles = articles.ToList();

        foreach (var article in _articles)
        {
            article.OnDestroyed.Subscribe(sender =>
            {
                _articles.Remove(sender);
            }).AddTo(article.gameObject);
        }
    }

    public bool NoOneId(short id)
    {
        return !_articles.Where(a => a.Article.Id == id).Any();
    }

    private List<ArticleContainer> _articles;
}
