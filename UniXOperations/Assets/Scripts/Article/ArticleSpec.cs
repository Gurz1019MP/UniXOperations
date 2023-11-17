using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ArticleSpec
{
    public short Id;
    public string Name;
    public float HitPoint;
    public Vector3 BottomPosition;
    public float JumpPower;
    public string ModelName;
    public string Texture;

    public static ArticleSpec GetSpec(short articleKind)
    {
        try
        {
            return JsonContainer.Instance.ArticleSpecArray.Single(s => s.Id == articleKind);
        }
        catch
        {
            Debug.LogError($"WeaponSpecの読み込みで定義されていないIdを読み込みました(Id:{articleKind})");
            return null;
        }
    }

    private static readonly Dictionary<short, ArticleSpec> SpecMapper = new Dictionary<short, ArticleSpec>()
    {
        { 0, new ArticleSpec(){ Name = "缶", HitPoint = 1f, BottomPosition = new Vector3(0, -0.054f, 0), JumpPower = 1f, ModelName = "can", Texture = "can" }},
        { 1, new ArticleSpec(){ Name = "パソコン", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc" }},
        { 2, new ArticleSpec(){ Name = "パソコン キーボード上", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc2", Texture = "pc" }},
        { 3, new ArticleSpec(){ Name = "パソコン 本体逆", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc3", Texture = "pc" }},
        { 4, new ArticleSpec(){ Name = "パソコン ワイド", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc_w", Texture = "pc" }},
        { 5, new ArticleSpec(){ Name = "パソコン ワイド キーボード上", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc2_w", Texture = "pc" }},
        { 6, new ArticleSpec(){ Name = "パソコン ワイド 本体逆", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc3_w", Texture = "pc" }},
        { 7, new ArticleSpec(){ Name = "椅子", HitPoint = 60f, BottomPosition = new Vector3(0, -0.256f, 0), JumpPower = 0.1f, ModelName = "isu", Texture = "isu" }},
        { 8, new ArticleSpec(){ Name = "ダンボール", HitPoint = 60f, BottomPosition = new Vector3(0, -0.177f, 0), JumpPower = 0.1f, ModelName = "dan", Texture = "dan" }},
        { 9, new ArticleSpec(){ Name = "パソコン 起動中", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc_sw" }},
        { 10, new ArticleSpec(){ Name = "パソコン 起動中 暗", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc_d" }},
        { 11, new ArticleSpec(){ Name = "パイロン", HitPoint = 30f, BottomPosition = new Vector3(0, -0.093f, 0), JumpPower = 0.5f, ModelName = "cone", Texture = "cone" }},
        { 12, new ArticleSpec(){ Name = "追加小物", HitPoint = 0f, BottomPosition = new Vector3(0, 0, 0), JumpPower = 0f, ModelName = null, Texture = null }},
    };
}
