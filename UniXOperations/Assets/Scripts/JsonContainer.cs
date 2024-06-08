using System;
using System.Collections.Generic;
using UnityEngine;
using static WeaponSpec;

[Serializable]
public class JsonContainer
{
    private static string path = System.IO.Path.Combine(Application.streamingAssetsPath, "JsonContainer.json");

    public JsonContainer()
    {
        //ArticleSpecArray = new ArticleSpec[]
        //{
        //    new ArticleSpec(){ Id = 0, Name = "缶", HitPoint = 1f, BottomPosition = new Vector3(0, -0.054f, 0), JumpPower = 1f, ModelName = "can", Texture = "can" },
        //    new ArticleSpec(){ Id = 1, Name = "パソコン", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc" },
        //    new ArticleSpec(){ Id = 2, Name = "パソコン キーボード上", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc2", Texture = "pc" },
        //    new ArticleSpec(){ Id = 3, Name = "パソコン 本体逆", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc3", Texture = "pc" },
        //    new ArticleSpec(){ Id = 4, Name = "パソコン ワイド", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc_w", Texture = "pc" },
        //    new ArticleSpec(){ Id = 5, Name = "パソコン ワイド キーボード上", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc2_w", Texture = "pc" },
        //    new ArticleSpec(){ Id = 6, Name = "パソコン ワイド 本体逆", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc3_w", Texture = "pc" },
        //    new ArticleSpec(){ Id = 7, Name = "椅子", HitPoint = 60f, BottomPosition = new Vector3(0, -0.256f, 0), JumpPower = 0.1f, ModelName = "isu", Texture = "isu" },
        //    new ArticleSpec(){ Id = 8, Name = "ダンボール", HitPoint = 60f, BottomPosition = new Vector3(0, -0.177f, 0), JumpPower = 0.1f, ModelName = "dan", Texture = "dan" },
        //    new ArticleSpec(){ Id = 9, Name = "パソコン 起動中", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc_sw" },
        //    new ArticleSpec(){ Id = 10, Name = "パソコン 起動中 暗", HitPoint = 60f, BottomPosition = new Vector3(0, -0.161f, 0), JumpPower = 0.1f, ModelName = "pc", Texture = "pc_d" },
        //    new ArticleSpec(){ Id = 11, Name = "パイロン", HitPoint = 30f, BottomPosition = new Vector3(0, -0.093f, 0), JumpPower = 0.5f, ModelName = "cone", Texture = "cone" },
        //    new ArticleSpec(){ Id = 12, Name = "追加小物", HitPoint = 0f, BottomPosition = new Vector3(0, 0, 0), JumpPower = 0f, ModelName = null, Texture = null },
        //};

        //WeaponSpecArray = new WeaponSpec[]
        //{
        //new WeaponSpec(){ Id = 0, WeaponName = "NONE", FirePower = 0f, FiringInterval = 0.1f, BulletSpeed = 100f, MagazineSize = 0, ReloadTime = 1f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Body, ModelName = null, FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = false },
        //new WeaponSpec(){ Id = 1, WeaponName = "MP5", FirePower = 40f, FiringInterval = 0.075f, BulletSpeed = 80f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "mp5", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0486f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0133f, 0.2117f), IsGrenade = false },
        //new WeaponSpec(){ Id = 2, WeaponName = "PSG1", FirePower = 80f, FiringInterval = 1f, BulletSpeed = 150f, MagazineSize = 5, ReloadTime = 4f, AccuracyMin = 0f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Rifle, ModelName = "psg1", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.Detail, HandlingPosition = new Vector3(0, -0.0226f, -0.1085f), MuzzlePosition = new Vector3(0, 0.0333f, 0.3825f), IsGrenade = false },
        //new WeaponSpec(){ Id = 3, WeaponName = "M92F", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 15, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "m92f", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.006f, -0.0388f), MuzzlePosition = new Vector3(0, 0.0443f, 0.0796f), IsGrenade = false },
        //new WeaponSpec(){ Id = 4, WeaponName = "GLOCK18 SEMI", FirePower = 40f, FiringInterval = 0.05f, BulletSpeed = 60f, MagazineSize = 19, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "glock18s", FireAudio = "fire2", ChangeFireModeWeapon = 16, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false },
        //new WeaponSpec(){ Id = 5, WeaponName = "DESERT EAGLE", FirePower = 65f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 7, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "de", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0332f, -0.0573f), MuzzlePosition = new Vector3(0, 0.0197f, 0.1015f), IsGrenade = false },
        //new WeaponSpec(){ Id = 6, WeaponName = "MAC10", FirePower = 40f, FiringInterval = 0.055f, BulletSpeed = 60f, MagazineSize = 30, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = true, Holding = HoldingMethod.Pistol, ModelName = "mac10", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0.0115f, -0.0103f), MuzzlePosition = new Vector3(0, 0.061f, 0.1021f), IsGrenade = false },
        //new WeaponSpec(){ Id = 7, WeaponName = "UMP", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 80f, MagazineSize = 25, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "ump", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0332f, -0.0266f), MuzzlePosition = new Vector3(0, 0.048f, 0.3314f), IsGrenade = false },
        //new WeaponSpec(){ Id = 8, WeaponName = "P90", FirePower = 65f, FiringInterval = 0.067f, BulletSpeed = 100f, MagazineSize = 50, ReloadTime = 5f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "p90", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0478f, -0.0203f), MuzzlePosition = new Vector3(0, 0.0141f, 0.2071f), IsGrenade = false },
        //new WeaponSpec(){ Id = 9, WeaponName = "M4", FirePower = 65f, FiringInterval = 0.067f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "m4", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.031f, -0.0789f), MuzzlePosition = new Vector3(0, 0.0508f, 0.3776f), IsGrenade = false },
        //new WeaponSpec(){ Id = 10, WeaponName = "AK47", FirePower = 65f, FiringInterval = 0.1f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "ak47", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.072f, -0.124f), MuzzlePosition = new Vector3(0, -0.014f, 0.375f), IsGrenade = false },
        //new WeaponSpec(){ Id = 11, WeaponName = "AUG", FirePower = 65f, FiringInterval = 0.071f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "aug", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.Equal, HandlingPosition = new Vector3(0, -0.054f, -0.062f), MuzzlePosition = new Vector3(0, 0.01f, 0.258f), IsGrenade = false },
        //new WeaponSpec(){ Id = 12, WeaponName = "M249", FirePower = 65f, FiringInterval = 0.075f, BulletSpeed = 110f, MagazineSize = 100, ReloadTime = 5f, AccuracyMin = 0.05f, AccuracyMax = 2f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "m249", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0936f, -0.0995f), MuzzlePosition = new Vector3(0, -0.0014f, 0.3673f), IsGrenade = false },
        //new WeaponSpec(){ Id = 13, WeaponName = "GRENADE", FirePower = 200f, FiringInterval = 1.5f, BulletSpeed = 10f, MagazineSize = 1, ReloadTime = 0f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "grenade", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "GrenadePrefab", BulletInstanceOffset = 0.5f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0074f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = true },
        //new WeaponSpec(){ Id = 14, WeaponName = "MP5SD", FirePower = 30f, FiringInterval = 0.075f, BulletSpeed = 70f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "mp5sd", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0486f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0106f, 0.2879f), IsGrenade = false },
        //new WeaponSpec(){ Id = 15, WeaponName = "CASE", FirePower = 0f, FiringInterval = 0.1f, BulletSpeed = 0f, MagazineSize = 0, ReloadTime = 1f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Body, ModelName = "case", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0.189f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = false },
        //new WeaponSpec(){ Id = 16, WeaponName = "GLOCK18 FULL", FirePower = 40f, FiringInterval = 0.05f, BulletSpeed = 60f, MagazineSize = 19, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = true, Holding = HoldingMethod.Pistol, ModelName = "glock18f", FireAudio = "fire2", ChangeFireModeWeapon = 4, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false },
        //new WeaponSpec(){ Id = 17, WeaponName = "M1911", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 7, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "cg", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.009f, -0.062f), MuzzlePosition = new Vector3(0, 0.0429f, 0.0612f), IsGrenade = false },
        //new WeaponSpec(){ Id = 18, WeaponName = "GLOCK17", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 17, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "glock17", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false },
        //new WeaponSpec(){ Id = 19, WeaponName = "M1", FirePower = 30f, FiringInterval = 1f, BulletSpeed = 50f, MagazineSize = 7, ReloadTime = 5f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Rifle, ModelName = "m1", FireAudio = "fire3", ChangeFireModeWeapon = 0, BulletCount = 6, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0032f, -0.1558f), MuzzlePosition = new Vector3(0, 0.0264f, 0.2981f), IsGrenade = false },
        //new WeaponSpec(){ Id = 20, WeaponName = "FAMAS", FirePower = 65f, FiringInterval = 0.055f, BulletSpeed = 100f, MagazineSize = 25, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "famas", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0541f, -0.0373f), MuzzlePosition = new Vector3(0, 0.0111f, 0.2354f), IsGrenade = false },
        //new WeaponSpec(){ Id = 21, WeaponName = "MK23", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 12, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "mk23", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0164f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0324f, 0.0891f), IsGrenade = false },
        //new WeaponSpec(){ Id = 22, WeaponName = "MK23 SD", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 50f, MagazineSize = 12, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "mk23sd", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0164f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0324f, 0.2376f), IsGrenade = false },
        //};

        //CharacterSpecArray = new CharacterSpec[]
        //{
        //new CharacterSpec(){ Id = 0, Name = "特殊 黒 A", AISkill = 4, Hitpoint = 150f, Weapon1 = 21, Weapon2 = 8, UpperModel = "up5", Texture = "soldier_black" },
        //new CharacterSpec(){ Id = 1, Name = "特殊 黒 B", AISkill = 4, Hitpoint = 150f, Weapon1 = 21, Weapon2 = 2, UpperModel = "up5", Texture = "soldier_black" },
        //new CharacterSpec(){ Id = 2, Name = "特殊 緑 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 6, UpperModel = "up0", Texture = "soldier_green" },
        //new CharacterSpec(){ Id = 3, Name = "特殊 緑 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 10, UpperModel = "up0", Texture = "soldier_green" },
        //new CharacterSpec(){ Id = 4, Name = "特殊 緑 C", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 2, UpperModel = "up0", Texture = "soldier_green" },
        //new CharacterSpec(){ Id = 5, Name = "特殊 白 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 16, Weapon2 = 7, UpperModel = "up0", Texture = "soldier_white" },
        //new CharacterSpec(){ Id = 6, Name = "特殊 白 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 16, Weapon2 = 2, UpperModel = "up0", Texture = "soldier_white" },
        //new CharacterSpec(){ Id = 7, Name = "ハゲ", AISkill = 4, Hitpoint = 100f, Weapon1 = 16, Weapon2 = 0, UpperModel = "up0", Texture = "hage" },
        //new CharacterSpec(){ Id = 8, Name = "特殊 紫", AISkill = 4, Hitpoint = 150f, Weapon1 = 22, Weapon2 = 14, UpperModel = "up0", Texture = "soldier_violet" },
        //new CharacterSpec(){ Id = 9, Name = "特殊 青", AISkill = 3, Hitpoint = 150f, Weapon1 = 3, Weapon2 = 1, UpperModel = "up0", Texture = "soldier_blue" },
        //new CharacterSpec(){ Id = 10, Name = "戦闘用ロボット", AISkill = 5, Hitpoint = 2000f, Weapon1 = 6, Weapon2 = 12, UpperModel = "up0", Texture = "robot" },
        //new CharacterSpec(){ Id = 11, Name = "スーツ 黒 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 18, Weapon2 = 0, UpperModel = "up1", Texture = "gs" },
        //new CharacterSpec(){ Id = 12, Name = "スーツ 黒", AISkill = 2, Hitpoint = 100f, Weapon1 = 5, Weapon2 = 0, UpperModel = "up0", Texture = "gs" },
        //new CharacterSpec(){ Id = 13, Name = "スーツ 灰", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_g" },
        //new CharacterSpec(){ Id = 14, Name = "警察官", AISkill = 2, Hitpoint = 100f, Weapon1 = 3, Weapon2 = 0, UpperModel = "up2", Texture = "police" },
        //new CharacterSpec(){ Id = 15, Name = "スーツ 茶", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman" },
        //new CharacterSpec(){ Id = 16, Name = "シャツ男 1", AISkill = 1, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up0", Texture = "syatu" },
        //new CharacterSpec(){ Id = 17, Name = "中東兵", AISkill = 2, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 10, UpperModel = "up0", Texture = "islam" },
        //new CharacterSpec(){ Id = 18, Name = "女", AISkill = 1, Hitpoint = 90f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up3", Texture = "woman" },
        //new CharacterSpec(){ Id = 19, Name = "金髪男", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ1" },
        //new CharacterSpec(){ Id = 20, Name = "市民 1", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ2" },
        //new CharacterSpec(){ Id = 21, Name = "市民 2", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "civ3" },
        //new CharacterSpec(){ Id = 22, Name = "シャツ男 1 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "syatu" },
        //new CharacterSpec(){ Id = 23, Name = "金髪男 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ1" },
        //new CharacterSpec(){ Id = 24, Name = "市民 1 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ2" },
        //new CharacterSpec(){ Id = 25, Name = "市民 2 SG", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up1", Texture = "civ3" },
        //new CharacterSpec(){ Id = 26, Name = "兵士 1 A", AISkill = 3, Hitpoint = 150f, Weapon1 = 5, Weapon2 = 9, UpperModel = "up4", Texture = "soldier0" },
        //new CharacterSpec(){ Id = 27, Name = "兵士 1 B", AISkill = 3, Hitpoint = 150f, Weapon1 = 5, Weapon2 = 12, UpperModel = "up4", Texture = "soldier1" },
        //new CharacterSpec(){ Id = 28, Name = "兵士 2", AISkill = 3, Hitpoint = 150f, Weapon1 = 13, Weapon2 = 10, UpperModel = "up4", Texture = "soldier2" },
        //new CharacterSpec(){ Id = 29, Name = "ゾンビ 1", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie1" },
        //new CharacterSpec(){ Id = 30, Name = "ゾンビ 2", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie2" },
        //new CharacterSpec(){ Id = 31, Name = "ゾンビ 3", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie3" },
        //new CharacterSpec(){ Id = 32, Name = "ゾンビ 4", AISkill = -1, Hitpoint = 1000f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "zombie4" },
        //new CharacterSpec(){ Id = 33, Name = "スーツ 紺", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_k" },
        //new CharacterSpec(){ Id = 34, Name = "スーツ 紺 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up1", Texture = "riiman_k" },
        //new CharacterSpec(){ Id = 35, Name = "将軍", AISkill = 2, Hitpoint = 90f, Weapon1 = 17, Weapon2 = 0, UpperModel = "up0", Texture = "islam2" },
        //new CharacterSpec(){ Id = 36, Name = "スーツ 青", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "riiman_b" },
        //new CharacterSpec(){ Id = 37, Name = "スーツ 青 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 18, Weapon2 = 0, UpperModel = "up1", Texture = "riiman_b" },
        //new CharacterSpec(){ Id = 38, Name = "シャツ男 2 SG", AISkill = 2, Hitpoint = 100f, Weapon1 = 6, Weapon2 = 0, UpperModel = "up1", Texture = "syatu2" },
        //new CharacterSpec(){ Id = 39, Name = "兵士 3", AISkill = 4, Hitpoint = 100f, Weapon1 = 5, Weapon2 = 20, UpperModel = "up0", Texture = "soldier3" },
        //new CharacterSpec(){ Id = 40, Name = "兵士 3 SG", AISkill = 4, Hitpoint = 100f, Weapon1 = 21, Weapon2 = 0, UpperModel = "up1", Texture = "soldier3" },
        //new CharacterSpec(){ Id = 41, Name = "ゲイツ", AISkill = 1, Hitpoint = 100f, Weapon1 = 0, Weapon2 = 0, UpperModel = "up0", Texture = "gates" },
        //new CharacterSpec(){ Id = 42, Name = "ゲイツ SG", AISkill = 5, Hitpoint = 100f, Weapon1 = 22, Weapon2 = 0, UpperModel = "up1", Texture = "gates" },
        //};

        //AISkillArray = new AISkill[]
        //{
        //new AISkill() { Id = 1, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 50f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0f, ShootThreshold = 20f, ShootError = 3f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        //new AISkill() { Id = 2, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0f, ShootThreshold = 10f, ShootError = 1.5f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        //new AISkill() { Id = 3, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 10f, ShootError = 1f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        //new AISkill() { Id = 4, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 10f, ShootError = 0.75f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        //new AISkill() { Id = 5, PropControlConstant = 0.03f, DetectionRange = 30f, DetectionAngleHorizontal = 80f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 3f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 0.5f, ShootError = 0.1f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new CombatAI(c) },
        //new AISkill() { Id = -1, PropControlConstant = 0.02f, DetectionRange = 20f, DetectionAngleHorizontal = 60f, DetectionAngleVertical = 30f, DetectionDelay = 0.5f, RunningDetectionRange = 2f, MissDelay = 3f, AlertTime = 5f, AimHeightOffset = 0.5f, ShootThreshold = 0.5f, ShootError = 0f, SafeAICreator = c => new PathAI(c), AlertAICreator = c => new AlertAI(c), CombatAICreator = c => new ZombieCombatAI(c) },
        //};
    }

    private static JsonContainer _instance;
    public static JsonContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                string text = System.IO.File.ReadAllText(path);
                _instance = JsonUtility.FromJson<JsonContainer>(text);

                //_instance = new JsonContainer();
            }

            return _instance;
        }
    }

    public static void Save()
    {
        using (var sw = System.IO.File.CreateText("D://temp.json"))
        {
            sw.Write(JsonUtility.ToJson(Instance, true));
        }
    }

    [SerializeField]
    public ArticleSpec[] ArticleSpecArray;

    [SerializeField]
    public WeaponSpec[] WeaponSpecArray;

    [SerializeField]
    public CharacterSpec[] CharacterSpecArray;

    [SerializeField]
    public AISkill[] AISkillArray;
}