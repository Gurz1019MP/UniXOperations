using System;
using UnityEngine;
using System.Linq;

[Serializable]
public struct ArticleSpec : IEquatable<ArticleSpec>
{
    static ArticleSpec()
    {
        Empty = new ArticleSpec(0, string.Empty, 0, Vector3.zero, 0, string.Empty, string.Empty, true);
    }

    public ArticleSpec(short id,
                       string name,
                       float hitPoint,
                       Vector3 bottomPosition,
                       float jumpPower,
                       string modelName,
                       string texture,
                       bool isEmpty)
    {
        Id = id;
        Name = name;
        HitPoint = hitPoint;
        BottomPosition = bottomPosition;
        JumpPower = jumpPower;
        ModelName = modelName;
        Texture = texture;
        _isEmpty = isEmpty;
    }

    public short Id;
    public string Name;
    public float HitPoint;
    public Vector3 BottomPosition;
    public float JumpPower;
    public string ModelName;
    public string Texture;

    private bool _isEmpty;

    public static ArticleSpec Empty { get; }

    public static ArticleSpec GetArticleSpec(short articleKind)
    {
        try
        {
            return articleSpecs.Single(s => s.Id == articleKind);
        }
        catch
        {
            Debug.LogError($"WeaponSpecの読み込みで定義されていないIdを読み込みました(Id:{articleKind})");
            return Empty;
        }
    }

    #region IEquatable

    public override bool Equals(object obj) => obj is ArticleSpec other && this.Equals(other);

    public bool Equals(ArticleSpec p)
    {
        return Id == p.Id && 
               Name == p.Name && 
               HitPoint == p.HitPoint && 
               BottomPosition == p.BottomPosition && 
               JumpPower == p.JumpPower &&
               ModelName == p.ModelName &&
               Texture == p.Texture &&
               _isEmpty == p._isEmpty;

    }

    public override int GetHashCode() => (Id, Name, HitPoint, BottomPosition, JumpPower, ModelName, Texture, _isEmpty).GetHashCode();

    public static bool operator ==(ArticleSpec lhs, ArticleSpec rhs) => lhs.Equals(rhs);

    public static bool operator !=(ArticleSpec lhs, ArticleSpec rhs) => !(lhs == rhs);

    #endregion

    private static readonly ArticleSpec[] articleSpecs = new ArticleSpec[]
    {
        new(0, "缶", 1f, new Vector3(0, -0.054f, 0), 1f, "can", "can", false),
        new(1, "パソコン", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc", "pc", false),
        new(2, "パソコン キーボード上", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc2", "pc", false),
        new(3, "パソコン 本体逆", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc3", "pc", false),
        new(4, "パソコン ワイド", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc_w", "pc", false),
        new(5, "パソコン ワイド キーボード上", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc2_w", "pc", false),
        new(6, "パソコン ワイド 本体逆", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc3_w", "pc", false),
        new(7, "椅子", 60f, new Vector3(0, -0.256f, 0), 0.1f, "isu", "isu", false),
        new(8, "ダンボール", 60f, new Vector3(0, -0.177f, 0), 0.1f, "dan", "dan", false),
        new(9, "パソコン 起動中", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc", "pc_sw", false),
        new(10, "パソコン 起動中 暗", 60f, new Vector3(0, -0.161f, 0), 0.1f, "pc", "pc_d", false),
        new(11, "パイロン", 30f, new Vector3(0, -0.093f, 0), 0.5f, "cone", "cone", false),
    };
}
