using System;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

[RequireComponent(typeof(Fallable))]
public class ArticleContainer : MonoBehaviour
{
    public Article Article { get; private set; }

    public IObservable<ArticleContainer> OnDestroyed => _onDestroyedSubject;

    public void InitArticleSpec(Article article)
    {
        Article = article;

        transform.position += new Vector3(0, 0.1f, 0);
        var fallable = GetComponent<Fallable>();

        // パラメータの設定
        _broken = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabBrokenArticle);
        fallable.IsGround = Article.IsGroundFlag;
        fallable.BottomTransfrom.localPosition = Article.Spec.BottomPosition;

        // モデルの読み込み
        if (!string.IsNullOrEmpty(Article.Spec.ModelName) && !string.IsNullOrEmpty(Article.Spec.Texture))
        {
            _visual = new VarialbeVisual(Article.Spec);
            _visual.ChangeMesh(gameObject);
            _visual.ChangeMaterial(gameObject);
        }
        else
        {
            Debug.Log("小物情報にモデル情報が登録されていません");
        }

        Article.OnDestroyed.Subscribe(_ =>
        {
            var instance = Instantiate(_broken, transform.position, transform.rotation);
            var broken = instance.GetComponent<BrokenArticle>();
            broken.Initialize(_visual, Article.Spec.JumpPower);

            _onDestroyedSubject.OnNext(this);
            _onDestroyedSubject.OnCompleted();
            Destroy(gameObject);
        }).AddTo(gameObject);
    }

    [SerializeField]
    private GameObject _broken;
    private VarialbeVisual _visual;
    private Subject<ArticleContainer> _onDestroyedSubject = new Subject<ArticleContainer>();
}
