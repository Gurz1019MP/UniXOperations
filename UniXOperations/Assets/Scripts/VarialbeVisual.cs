using UnityEngine;

public readonly struct VarialbeVisual
{
    #region .ctor

    public VarialbeVisual(Mesh mesh,
                      Material material,
                      Texture2D texture)
    {
        _mesh = mesh;
        _material = material;
        _texture = texture;
    }

    public VarialbeVisual(CharacterSpec characterSpec) : this(characterSpec.GetMesh(), CharacterSpec.GetMaterial(), characterSpec.GetTexture()) { }

    #endregion

    #region Method

    public void ChangeMesh(GameObject target)
    {
        if (target == null) return;

        var meshFilterComponent = target.GetComponent<MeshFilter>();
        var meshColliderComponent = target.GetComponent<MeshCollider>();
        if (meshFilterComponent == null)
        {
            Debug.Log("メッシュの読み込みに失敗");
            return;
        }

        meshFilterComponent.mesh = _mesh;

        if (meshColliderComponent != null)
        {
            meshColliderComponent.sharedMesh = _mesh;
        }
    }

    public void ChangeMaterial(GameObject target)
    {
        if (target == null) return;

        var component = target.GetComponent<Renderer>();
        if (component == null)
        {
            Debug.Log("マテリアルの読み込みに失敗");
            return;
        }

        component.material = _material;
        component.material.mainTexture = _texture;
    }

    #endregion

    #region Field

    private readonly Mesh _mesh;
    private readonly Material _material;
    private readonly Texture2D _texture;

    #endregion
}
