using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class WeaponSpec
{
    public short Id;
    public string WeaponName;
    public float FirePower;
    public float PenetrationPower;
    public float FiringInterval;
    public float BulletSpeed;
    public int MagazineSize;
    public float ReloadTime;
    public float RecoilAngle;
    public float AccuracyMin;
    public float AccuracyMax;
    public bool FullAuto;
    public ZoomModeEnum Scope;
    public HoldingMethod Holding;
    public string ModelName;
    public string FireAudio;
    public float Sound;
    public short ChangeFireModeWeapon;
    public short BulletCount;
    public string BulletName;
    public float BulletInstanceOffset;
    public Vector3 HandlingPosition;
    public Vector3 MuzzlePosition;
    public bool IsGrenade;
    public float MuzzleJump;

    public static WeaponSpec GetSpec(short weaponKind)
    {
        try
        {
            return JsonContainer.Instance.WeaponSpecArray.Single(s => s.Id == weaponKind);
        }
        catch
        {
            Debug.LogError($"WeaponSpecの読み込みで定義されていないIdを読み込みました(Id:{weaponKind})");
            return null;
        }
    }

    public enum HoldingMethod
    {
        Body,
        Rifle,
        Pistol
    }

    public enum ZoomModeEnum
    {
        None,
        Equal,
        Low,
        Detail
    }

    private static readonly Dictionary<short, WeaponSpec> SpeckMapper = new Dictionary<short, WeaponSpec>()
    {
        { 0, new WeaponSpec(){ WeaponName = "NONE", FirePower = 0f, FiringInterval = 0.1f, BulletSpeed = 100f, MagazineSize = 0, ReloadTime = 1f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Body, ModelName = "null", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = false, MuzzleJump = 0 }},
        { 1, new WeaponSpec(){ WeaponName = "MP5", FirePower = 40f, FiringInterval = 0.075f, BulletSpeed = 80f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "mp5", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0486f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0133f, 0.2117f), IsGrenade = false, MuzzleJump = 5 }},
        { 2, new WeaponSpec(){ WeaponName = "PSG1", FirePower = 80f, FiringInterval = 1f, BulletSpeed = 150f, MagazineSize = 5, ReloadTime = 4f, AccuracyMin = 0f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Rifle, ModelName = "psg1", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.Detail, HandlingPosition = new Vector3(0, -0.0226f, -0.1085f), MuzzlePosition = new Vector3(0, 0.0333f, 0.3825f), IsGrenade = false, MuzzleJump = 10 }},
        { 3, new WeaponSpec(){ WeaponName = "M92F", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 15, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "m92f", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.006f, -0.0388f), MuzzlePosition = new Vector3(0, 0.0443f, 0.0796f), IsGrenade = false, MuzzleJump = 7 }},
        { 4, new WeaponSpec(){ WeaponName = "GLOCK18 SEMI", FirePower = 40f, FiringInterval = 0.05f, BulletSpeed = 60f, MagazineSize = 19, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "glock18s", FireAudio = "fire2", ChangeFireModeWeapon = 16, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false, MuzzleJump = 7 }},
        { 5, new WeaponSpec(){ WeaponName = "DESERT EAGLE", FirePower = 65f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 7, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "de", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0332f, -0.0573f), MuzzlePosition = new Vector3(0, 0.0197f, 0.1015f), IsGrenade = false, MuzzleJump = 10 }},
        { 6, new WeaponSpec(){ WeaponName = "MAC10", FirePower = 40f, FiringInterval = 0.055f, BulletSpeed = 60f, MagazineSize = 30, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = true, Holding = HoldingMethod.Pistol, ModelName = "mac10", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0.0115f, -0.0103f), MuzzlePosition = new Vector3(0, 0.061f, 0.1021f), IsGrenade = false, MuzzleJump = 7 }},
        { 7, new WeaponSpec(){ WeaponName = "UMP", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 80f, MagazineSize = 25, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "ump", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0332f, -0.0266f), MuzzlePosition = new Vector3(0, 0.048f, 0.3314f), IsGrenade = false, MuzzleJump = 5 }},
        { 8, new WeaponSpec(){ WeaponName = "P90", FirePower = 65f, FiringInterval = 0.067f, BulletSpeed = 100f, MagazineSize = 50, ReloadTime = 5f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "p90", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0478f, -0.0203f), MuzzlePosition = new Vector3(0, 0.0141f, 0.2071f), IsGrenade = false, MuzzleJump = 5 }},
        { 9, new WeaponSpec(){ WeaponName = "M4", FirePower = 65f, FiringInterval = 0.067f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "m4", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.031f, -0.0789f), MuzzlePosition = new Vector3(0, 0.0508f, 0.3776f), IsGrenade = false, MuzzleJump = 5 }},
        { 10, new WeaponSpec(){ WeaponName = "AK47", FirePower = 65f, FiringInterval = 0.1f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "ak47", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.072f, -0.124f), MuzzlePosition = new Vector3(0, -0.014f, 0.375f), IsGrenade = false, MuzzleJump = 5 }},
        { 11, new WeaponSpec(){ WeaponName = "AUG", FirePower = 65f, FiringInterval = 0.071f, BulletSpeed = 100f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "aug", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.Equal, HandlingPosition = new Vector3(0, -0.054f, -0.062f), MuzzlePosition = new Vector3(0, 0.01f, 0.258f), IsGrenade = false, MuzzleJump = 5 }},
        { 12, new WeaponSpec(){ WeaponName = "M249", FirePower = 65f, FiringInterval = 0.075f, BulletSpeed = 110f, MagazineSize = 100, ReloadTime = 5f, AccuracyMin = 0.05f, AccuracyMax = 2f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "m249", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0936f, -0.0995f), MuzzlePosition = new Vector3(0, -0.0014f, 0.3673f), IsGrenade = false, MuzzleJump = 4 }},
        { 13, new WeaponSpec(){ WeaponName = "GRENADE", FirePower = 200f, FiringInterval = 1.5f, BulletSpeed = 10f, MagazineSize = 1, ReloadTime = 0f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "grenade", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "GrenadePrefab", BulletInstanceOffset = 0.5f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0074f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = true, MuzzleJump = 0 }},
        { 14, new WeaponSpec(){ WeaponName = "MP5SD", FirePower = 30f, FiringInterval = 0.075f, BulletSpeed = 70f, MagazineSize = 30, ReloadTime = 2f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "mp5sd", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0486f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0106f, 0.2879f), IsGrenade = false, MuzzleJump = 5 }},
        { 15, new WeaponSpec(){ WeaponName = "CASE", FirePower = 0f, FiringInterval = 0.1f, BulletSpeed = 0f, MagazineSize = 0, ReloadTime = 1f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Body, ModelName = "case", FireAudio = "fire1", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, 0.189f, 0f), MuzzlePosition = new Vector3(0, 0f, 0f), IsGrenade = false, MuzzleJump = 0 }},
        { 16, new WeaponSpec(){ WeaponName = "GLOCK18 FULL", FirePower = 40f, FiringInterval = 0.05f, BulletSpeed = 60f, MagazineSize = 19, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = true, Holding = HoldingMethod.Pistol, ModelName = "glock18f", FireAudio = "fire2", ChangeFireModeWeapon = 4, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false, MuzzleJump = 7 }},
        { 17, new WeaponSpec(){ WeaponName = "M1911", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 7, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "cg", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.009f, -0.062f), MuzzlePosition = new Vector3(0, 0.0429f, 0.0612f), IsGrenade = false, MuzzleJump = 7 }},
        { 18, new WeaponSpec(){ WeaponName = "GLOCK17", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 17, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "glock17", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0123f, -0.0358f), MuzzlePosition = new Vector3(0, 0.0377f, 0.0786f), IsGrenade = false, MuzzleJump = 7 }},
        { 19, new WeaponSpec(){ WeaponName = "M1", FirePower = 30f, FiringInterval = 1f, BulletSpeed = 50f, MagazineSize = 7, ReloadTime = 5f, AccuracyMin = 0.2f, AccuracyMax = 5f, FullAuto = false, Holding = HoldingMethod.Rifle, ModelName = "m1", FireAudio = "fire3", ChangeFireModeWeapon = 0, BulletCount = 6, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0032f, -0.1558f), MuzzlePosition = new Vector3(0, 0.0264f, 0.2981f), IsGrenade = false, MuzzleJump = 20 }},
        { 20, new WeaponSpec(){ WeaponName = "FAMAS", FirePower = 65f, FiringInterval = 0.055f, BulletSpeed = 100f, MagazineSize = 25, ReloadTime = 2f, AccuracyMin = 0.1f, AccuracyMax = 5f, FullAuto = true, Holding = HoldingMethod.Rifle, ModelName = "famas", FireAudio = "fire4", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0541f, -0.0373f), MuzzlePosition = new Vector3(0, 0.0111f, 0.2354f), IsGrenade = false, MuzzleJump = 5 }},
        { 21, new WeaponSpec(){ WeaponName = "MK23", FirePower = 50f, FiringInterval = 0.1f, BulletSpeed = 60f, MagazineSize = 12, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "mk23", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0164f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0324f, 0.0891f), IsGrenade = false, MuzzleJump = 7 }},
        { 22, new WeaponSpec(){ WeaponName = "MK23 SD", FirePower = 40f, FiringInterval = 0.1f, BulletSpeed = 50f, MagazineSize = 12, ReloadTime = 1.5f, AccuracyMin = 0.3f, AccuracyMax = 10f, FullAuto = false, Holding = HoldingMethod.Pistol, ModelName = "mk23sd", FireAudio = "fire2", ChangeFireModeWeapon = 0, BulletCount = 1, BulletName = "BulletPrefab", BulletInstanceOffset = 0f, Scope = ZoomModeEnum.None, HandlingPosition = new Vector3(0, -0.0164f, -0.0507f), MuzzlePosition = new Vector3(0, 0.0324f, 0.2376f), IsGrenade = false, MuzzleJump = 7 }},
    };
}
