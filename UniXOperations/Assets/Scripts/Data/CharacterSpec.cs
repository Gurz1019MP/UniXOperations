using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public struct CharacterSpec : IEquatable<CharacterSpec>
{
    static CharacterSpec()
    {
        _empty = new CharacterSpec(string.Empty, 0, 0f, 0, 0, string.Empty, string.Empty, true);
    }
 
    private CharacterSpec(string name,
                         short aiSkill,
                         float hitpoint,
                         short weapon1,
                         short weapon2,
                         string upperModel,
                         string texture,
                         bool isEmpty = false)
    {
        this.Name = name;
        this.AISkill = AISpec.GetAISkill(aiSkill);
        this.Hitpoint = hitpoint;
        this.Weapon1 = weapon1;
        this.Weapon2 = weapon2;
        this._upperModel = upperModel;
        this._texture = texture;
        this._isEmpty = isEmpty;
    }

    public string Name { get; }
    public AISpec AISkill { get; }
    public float Hitpoint { get; }
    public short Weapon1 { get; }
    public short Weapon2 { get; }

    public static CharacterSpec GetCharacterSpec(CharacterKind kind)
    {
        try
        {
            return CharacterSpecMapper[kind];
        }
        catch
        {
            Debug.LogError($"CharacterSpecの読み込みで定義されていないIdを読み込みました(Id:{kind})");
            return _empty;
        }
    }

    public Mesh GetMesh()
    {
        if (string.IsNullOrEmpty(_upperModel)) throw new ArgumentException("人情報に上半身メッシュ情報が登録されていません");

        return AssetLoader.LoadAsset<Mesh>(string.Format(_characterMeshResourceFormat, _upperModel));
    }

    public static Material GetMaterial()
    {
        Material material = AssetLoader.LoadAsset<Material>(_characterMaterialResource);
        material.shader = Shader.Find("Unlit/Texture");

        return material;
    }

    public Texture2D GetTexture()
    {
        if (string.IsNullOrEmpty(_texture)) throw new ArgumentException("人情報に上半身テクスチャ情報が登録されていません");

        return AssetLoader.LoadAsset<Texture2D>(string.Format(_characterTextureResourceFormat, _texture));
    }

    #region IEquatable

    public override bool Equals(object obj) => obj is CharacterSpec other && this.Equals(other);

    public bool Equals(CharacterSpec p) => Name == p.Name && AISkill == p.AISkill && Hitpoint == p.Hitpoint && Weapon1 == p.Weapon1 && Weapon2 == p.Weapon2;

    public override int GetHashCode() => (Name, AISkill, Hitpoint, Weapon1, Weapon2).GetHashCode();

    public static bool operator ==(CharacterSpec lhs, CharacterSpec rhs) => lhs.Equals(rhs);

    public static bool operator !=(CharacterSpec lhs, CharacterSpec rhs) => !(lhs == rhs);

    #endregion

    private static readonly Dictionary<CharacterKind, CharacterSpec> CharacterSpecMapper = new Dictionary<CharacterKind, CharacterSpec>()
    {
        { new CharacterKind(0), new("特殊 黒 A", 4, 150f, 21, 8, "up5", "soldier_black")},
        { new CharacterKind(1), new("特殊 黒 B", 4, 150f, 21, 2, "up5", "soldier_black")},
        { new CharacterKind(2), new("特殊 緑 A", 3, 150f, 3, 6, "up0", "soldier_green")},
        { new CharacterKind(3), new("特殊 緑 B", 3, 150f, 3, 10, "up0", "soldier_green")},
        { new CharacterKind(4), new("特殊 緑 C", 3, 150f, 3, 2, "up0", "soldier_green")},
        { new CharacterKind(5), new("特殊 白 A", 3, 150f, 16, 7, "up0", "soldier_white")},
        { new CharacterKind(6), new("特殊 白 B", 3, 150f, 16, 2, "up0", "soldier_white")},
        { new CharacterKind(7), new("ハゲ", 4, 100f, 16, 0, "up0", "hage")},
        { new CharacterKind(8), new("特殊 紫", 4, 150f, 22, 14, "up0", "soldier_violet")},
        { new CharacterKind(9), new("特殊 青", 3, 150f, 3, 1, "up0", "soldier_blue")},
        { new CharacterKind(10), new("戦闘用ロボット", 5, 2000f, 6, 12, "up0", "robot")},
        { new CharacterKind(11), new("スーツ 黒 SG", 2, 100f, 18, 0, "up1", "gs")},
        { new CharacterKind(12), new("スーツ 黒", 2, 100f, 5, 0, "up0", "gs")},
        { new CharacterKind(13), new("スーツ 灰", 1, 100f, 0, 0, "up0", "riiman_g")},
        { new CharacterKind(14), new("警察官", 2, 100f, 3, 0, "up2", "police")},
        { new CharacterKind(15), new("スーツ 茶", 1, 100f, 0, 0, "up0", "riiman")},
        { new CharacterKind(16), new("シャツ男 1", 1, 100f, 17, 0, "up0", "syatu")},
        { new CharacterKind(17), new("中東兵", 2, 100f, 17, 10, "up0", "islam")},
        { new CharacterKind(18), new("女", 1, 90f, 0, 0, "up3", "woman")},
        { new CharacterKind(19), new("金髪男", 1, 100f, 0, 0, "up0", "civ1")},
        { new CharacterKind(20), new("市民 1", 1, 100f, 0, 0, "up0", "civ2")},
        { new CharacterKind(21), new("市民 2", 1, 100f, 0, 0, "up0", "civ3")},
        { new CharacterKind(22), new("シャツ男 1 SG", 1, 100f, 0, 0, "up1", "syatu")},
        { new CharacterKind(23), new("金髪男 SG", 1, 100f, 0, 0, "up1", "civ1")},
        { new CharacterKind(24), new("市民 1 SG", 1, 100f, 0, 0, "up1", "civ2")},
        { new CharacterKind(25), new("市民 2 SG", 1, 100f, 0, 0, "up1", "civ3")},
        { new CharacterKind(26), new("兵士 1 A", 3, 150f, 5, 9, "up4", "soldier0")},
        { new CharacterKind(27), new("兵士 1 B", 3, 150f, 5, 12, "up4", "soldier1")},
        { new CharacterKind(28), new("兵士 2", 3, 150f, 13, 10, "up4", "soldier2")},
        { new CharacterKind(29), new("ゾンビ 1", -1, 1000f, 0, 0, "up0", "zombie1")},
        { new CharacterKind(30), new("ゾンビ 2", -1, 1000f, 0, 0, "up0", "zombie2")},
        { new CharacterKind(31), new("ゾンビ 3", -1, 1000f, 0, 0, "up0", "zombie3")},
        { new CharacterKind(32), new("ゾンビ 4", -1, 1000f, 0, 0, "up0", "zombie4")},
        { new CharacterKind(33), new("スーツ 紺", 1, 100f, 0, 0, "up0", "riiman_k")},
        { new CharacterKind(34), new("スーツ 紺 SG", 2, 100f, 17, 0, "up1", "riiman_k")},
        { new CharacterKind(35), new("将軍", 2, 90f, 17, 0, "up0", "islam2")},
        { new CharacterKind(36), new("スーツ 青", 1, 100f, 0, 0, "up0", "riiman_b")},
        { new CharacterKind(37), new("スーツ 青 SG", 2, 100f, 18, 0, "up1", "riiman_b")},
        { new CharacterKind(38), new("シャツ男 2 SG", 2, 100f, 6, 0, "up1", "syatu2")},
        { new CharacterKind(39), new("兵士 3", 4, 100f, 5, 20, "up0", "soldier3")},
        { new CharacterKind(40), new("兵士 3 SG", 4, 100f, 21, 0, "up1", "soldier3")},
        { new CharacterKind(41), new("ゲイツ", 1, 100f, 0, 0, "up0", "gates")},
        { new CharacterKind(42), new("ゲイツ SG", 5, 100f, 22, 0, "up1", "gates")},
    };

    private static readonly string _characterMeshResourceFormat = "Assets/Models/Character/Built-in/{0}.fbx";

    private static readonly string _characterMaterialResource = "Assets/Models/Character/Built-in/Material.mat";

    private static readonly string _characterTextureResourceFormat = "Assets/Models/Character/{0}.bmp";

    private static readonly CharacterSpec _empty;

    private readonly bool _isEmpty;
    private readonly string _upperModel;
    private readonly string _texture;
}
