using System.Collections.Generic;
using System.Linq;
using UniRx;

public class Articles
{
    public Articles(Article[] articles)
    {
        _articles = articles.ToList();

        foreach (var article in _articles)
        {
            article.OnDestroyed.Subscribe(sender =>
            {
                _articles.Remove(article);
            });
        }
    }

    public bool NoOneId(short id)
    {
        return !_articles.Where(a => a.Id == id).Any();
    }

    private List<Article> _articles;
}
