using System;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

[RequireComponent(typeof(Fallable))]
public class ArticleContainer : MonoBehaviour
{
    public IObservable<ArticleContainer> OnDestroyed => _onDestroyedSubject;

    public Article Article => _article;

    public void InitArticleSpec(PointData pointData)
    {
        transform.position += new Vector3(0, 0.1f, 0);
        var fallable = GetComponent<Fallable>();
        _spec = ArticleSpec.GetSpec(pointData.Data2);

        // パラメータの設定
        _article = new Article(pointData.Data4, _spec);
        _broken = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabBrokenArticle);
        fallable.IsGround = pointData.Data3 == 0;
        fallable.BottomTransfrom.localPosition = _spec.BottomPosition;

        // モデルの読み込み
        if (!string.IsNullOrEmpty(_spec.ModelName) && !string.IsNullOrEmpty(_spec.Texture))
        {
            // メッシュの読み込み
            _mesh = AssetLoader.LoadAsset<Mesh>(ConstantsManager.GetResoucePathArticleModel(_spec.ModelName));
            VisualChanger.ChangeMesh(gameObject, _mesh);

            // マテリアルの読み込み
            _material = AssetLoader.LoadAsset<Material>(ConstantsManager.GetResoucePathArticleModel(_spec.ModelName));
            _texture = AssetLoader.LoadAsset<Texture2D>(ConstantsManager.GetResoucePathArticleTexture(_spec.Texture));
            VisualChanger.ChangeMaterial(gameObject, _material, _texture);
        }
        else
        {
            Debug.Log("小物情報にモデル情報が登録されていません");
        }

        Article.OnDestroyed.Subscribe(_ =>
        {
            var instance = Instantiate(_broken, transform.position, transform.rotation);
            var broken = instance.GetComponent<BrokenArticle>();
            broken.Initialize(_mesh, _material, _texture, _spec.JumpPower);

            _onDestroyedSubject.OnNext(this);
            _onDestroyedSubject.OnCompleted();
            Destroy(gameObject);
        }).AddTo(gameObject);
    }

    [SerializeField]
    private Article _article;
    private ArticleSpec _spec;
    private GameObject _broken;
    private Mesh _mesh;
    private Material _material;
    private Texture2D _texture;
    private Subject<ArticleContainer> _onDestroyedSubject = new Subject<ArticleContainer>();
}
