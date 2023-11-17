using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharacterSpec
{
    public short Id;
    public string Name;
    public short AISkill;
    public float Hitpoint;
    public short Weapon1;
    public short Weapon2;
    public string UpperModel;
    public string Texture;

    public bool IsZombie => AISkill == -1;

    public static CharacterSpec GetCharacterSpec(short kind)
    {
        try
        {
            return JsonContainer.Instance.CharacterSpecArray.Single(s => s.Id == kind);
        }
        catch
        {
            Debug.LogError($"CharacterSpecの読み込みで定義されていないIdを読み込みました(Id:{kind})");
            return null;
        }
    }

    private static Dictionary<short, CharacterSpec> CharacterSpecMapper = new Dictionary<short, CharacterSpec>()
    {
        { 0, new CharacterSpec(){ Name = "特殊 黒 A", AISkill = 4, Hitpoint = 150f, Weapon1 = 21, Weapon2 = 8, UpperModel = "up5", Texture = "soldier_black" }},
        { 1, new CharacterSpec(){ Name = "特殊 黒 B", AISkill = 4, Hitpoint = 150f, Weapon1 = 21, Weapon2 = 2, UpperModel = "up5", Texture = "soldier_black" }},
        { 2, new CharacterSpec(){ Name = "特殊 緑 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 6, UpperModel = "up0", Texture = "soldier_green" }},
        { 3, new CharacterSpec(){ Name = "特殊 緑 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 10, UpperModel = "up0", Texture = "soldier_green" }},
        { 4, new CharacterSpec(){ Name = "特殊 緑 C", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 2, UpperModel = "up0", Texture = "soldier_green" }},
        { 5, new CharacterSpec(){ Name = "特殊 白 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 16, Weapon2 = 7, UpperModel = "up0", Texture = "soldier_white" }},
        { 6, new CharacterSpec(){ Name = "特殊 白 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 16, Weapon2 = 2, UpperModel = "up0", Texture = "soldier_white" }},
        { 7, new CharacterSpec(){ Name = "ハゲ", AISkill = 4, Hitpoint = 100f, Weapon1 = 16, Weapon2 = 0, UpperModel = "up0", Texture = "hage" }},
        { 8, new CharacterSpec(){ Name = "特殊 紫", AISkill = 4, Hitpoint = 150f, Weapon1 = 22, Weapon2 = 14, UpperModel = "up0", Texture = "soldier_violet" }},
        { 9, new CharacterSpec(){ Name = "特殊 青", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 1, UpperModel = "up0", Texture = "soldier_blue" }},
        { 10, new CharacterSpec(){ Name = "戦闘用ロボット", AISkill = 5, Hitpoint = 2000f, Weapon1 = 6, Weapon2 = 12, UpperModel = "up0", Texture = "robot" }},
        { 11, new CharacterSpec(){ Name = "スーツ 黒 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 18, Weapon2 = 0, UpperModel = "up1", Texture = "gs" }},
        { 12, new CharacterSpec(){ Name = "スーツ 黒", AISkill = 2, Hitpoint = 100f, Weapon1 = 5, Weapon2 = 0, UpperModel = "up0", Texture = "gs" }},
        { 13, new CharacterSpec(){ Name = "スーツ 灰", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_g" }},
        { 14, new CharacterSpec(){ Name = "警察官", AISkill = 2, Hitpoint = 100f, Weapon1 = 3, Weapon2 = 0, UpperModel = "up2", Texture = "police" }},
        { 15, new CharacterSpec(){ Name = "スーツ 茶", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman" }},
        { 16, new CharacterSpec(){ Name = "シャツ男 1", AISkill = 1, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up0", Texture = "syatu" }},
        { 17, new CharacterSpec(){ Name = "中東兵", AISkill = 2, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 10, UpperModel = "up0", Texture = "islam" }},
        { 18, new CharacterSpec(){ Name = "女", AISkill = 1, Hitpoint = 90f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up3", Texture = "woman" }},
        { 19, new CharacterSpec(){ Name = "金髪男", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ1" }},
        { 20, new CharacterSpec(){ Name = "市民 1", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ2" }},
        { 21, new CharacterSpec(){ Name = "市民 2", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ3" }},
        { 22, new CharacterSpec(){ Name = "シャツ男 1 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "syatu" }},
        { 23, new CharacterSpec(){ Name = "金髪男 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ1" }},
        { 24, new CharacterSpec(){ Name = "市民 1 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ2" }},
        { 25, new CharacterSpec(){ Name = "市民 2 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ3" }},
        { 26, new CharacterSpec(){ Name = "兵士 1 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 5, Weapon2 = 9, UpperModel = "up4", Texture = "soldier0" }},
        { 27, new CharacterSpec(){ Name = "兵士 1 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 5, Weapon2 = 12, UpperModel = "up4", Texture = "soldier1" }},
        { 28, new CharacterSpec(){ Name = "兵士 2", AISkill = 3, Hitpoint = 150f, Weapon1 = 13, Weapon2 = 10, UpperModel = "up4", Texture = "soldier2" }},
        { 29, new CharacterSpec(){ Name = "ゾンビ 1", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie1" }},
        { 30, new CharacterSpec(){ Name = "ゾンビ 2", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie2" }},
        { 31, new CharacterSpec(){ Name = "ゾンビ 3", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie3" }},
        { 32, new CharacterSpec(){ Name = "ゾンビ 4", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie4" }},
        { 33, new CharacterSpec(){ Name = "スーツ 紺", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_k" }},
        { 34, new CharacterSpec(){ Name = "スーツ 紺 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up1", Texture = "riiman_k" }},
        { 35, new CharacterSpec(){ Name = "将軍", AISkill = 2, Hitpoint = 90f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up0", Texture = "islam2" }},
        { 36, new CharacterSpec(){ Name = "スーツ 青", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_b" }},
        { 37, new CharacterSpec(){ Name = "スーツ 青 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 18, Weapon2 = 0, UpperModel = "up1", Texture = "riiman_b" }},
        { 38, new CharacterSpec(){ Name = "シャツ男 2 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 6, Weapon2 = 0, UpperModel = "up1", Texture = "syatu2" }},
        { 39, new CharacterSpec(){ Name = "兵士 3", AISkill = 4, Hitpoint = 100f, Weapon1 = 5, Weapon2 = 20, UpperModel = "up0", Texture = "soldier3" }},
        { 40, new CharacterSpec(){ Name = "兵士 3 SG", AISkill = 4, Hitpoint = 100f, Weapon1 = 21, Weapon2 = 0, UpperModel = "up1", Texture = "soldier3" }},
        { 41, new CharacterSpec(){ Name = "ゲイツ", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "gates" }},
        { 42, new CharacterSpec(){ Name = "ゲイツ SG", AISkill = 5, Hitpoint = 100f, Weapon1 = 22, Weapon2 = 0, UpperModel = "up1", Texture = "gates" }},
    };
}
