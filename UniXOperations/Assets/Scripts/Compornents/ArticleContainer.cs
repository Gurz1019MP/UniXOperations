using System;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

[RequireComponent(typeof(Fallable))]
public class ArticleContainer : MonoBehaviour
{
    public IObservable<ArticleContainer> OnDestroyed => _onDestroyedSubject;

    public ArticleState Article => _article;

    public void InitArticleSpec(PointData pointData)
    {
        transform.position += new Vector3(0, 0.1f, 0);
        var fallable = GetComponent<Fallable>();
        _spec = ArticleSpec.GetArticleSpec(pointData.Data2);

        // パラメータの設定
        _article = new ArticleState(pointData.Data4, _spec);
        _broken = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabBrokenArticle);
        fallable.IsGround = pointData.Data3 == 0;
        fallable.BottomTransfrom.localPosition = _spec.BottomPosition;

        // モデルの読み込み
        if (!string.IsNullOrEmpty(_spec.ModelName) && !string.IsNullOrEmpty(_spec.Texture))
        {
            _visual = new VarialbeVisual(_spec.Mesh, _spec.Material, _spec.Texture2D);
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
            broken.Initialize(_visual, _spec.JumpPower);

            _onDestroyedSubject.OnNext(this);
            _onDestroyedSubject.OnCompleted();
            Destroy(gameObject);
        }).AddTo(gameObject);
    }

    [SerializeField]
    private ArticleState _article;
    private ArticleSpec _spec;
    private GameObject _broken;
    private VarialbeVisual _visual;
    private Subject<ArticleContainer> _onDestroyedSubject = new Subject<ArticleContainer>();
}
