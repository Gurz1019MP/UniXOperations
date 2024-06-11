using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.TextCore.Text;

[Serializable]
public struct WeaponSpec : IEquatable<WeaponSpec>
{
    public WeaponSpec(short id,
                      string weaponName,
                      float firePower,
                      float penetrationPower,
                      float firingInterval,
                      float bulletSpeed,
                      int magazineSize,
                      float reloadTime,
                      float recoilAngle,
                      float accuracyMin,
                      float accuracyMax,
                      bool fullAuto,
                      ZoomModeEnum scope,
                      HoldingMethod holding,
                      string modelName,
                      string fireAudio,
                      float sound,
                      short changeFireModeWeapon,
                      short bulletCount,
                      Vector3 handlingPosition,
                      Vector3 muzzlePosition,
                      bool isGrenade,
                      float muzzleJump)
    {
        Id = id;
        WeaponName = weaponName;
        FirePower = firePower;
        PenetrationPower = penetrationPower;
        FiringInterval = firingInterval;
        BulletSpeed = bulletSpeed;
        MagazineSize = magazineSize;
        ReloadTime = reloadTime;
        RecoilAngle = recoilAngle;
        AccuracyMin = accuracyMin;
        AccuracyMax = accuracyMax;
        FullAuto = fullAuto;
        Scope = scope;
        Holding = holding;
        ModelName = modelName;
        FireAudio = fireAudio;
        Sound = sound;
        ChangeFireModeWeapon = changeFireModeWeapon;
        BulletCount = bulletCount;
        HandlingPosition = handlingPosition;
        MuzzlePosition = muzzlePosition;
        IsGrenade = isGrenade;
        MuzzleJump = muzzleJump;
    }

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
    public Vector3 HandlingPosition;
    public Vector3 MuzzlePosition;
    public bool IsGrenade;
    public float MuzzleJump;

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

    #region IEquatable

    public override bool Equals(object obj) => obj is WeaponSpec other && this.Equals(other);

    public bool Equals(WeaponSpec p)
    {
        return Id == p.Id &&
               WeaponName == p.WeaponName &&
               FirePower == p.FirePower &&
               PenetrationPower == p.PenetrationPower &&
               FiringInterval == p.FiringInterval &&
               BulletSpeed == p.BulletSpeed &&
               MagazineSize == p.MagazineSize &&
               ReloadTime == p.ReloadTime &&
               RecoilAngle == p.RecoilAngle &&
               AccuracyMin == p.AccuracyMin &&
               AccuracyMax == p.AccuracyMax &&
               FullAuto == p.FullAuto &&
               Scope == p.Scope &&
               Holding == p.Holding &&
               ModelName == p.ModelName &&
               FireAudio == p.FireAudio &&
               Sound == p.Sound &&
               ChangeFireModeWeapon == p.ChangeFireModeWeapon &&
               BulletCount == p.BulletCount &&
               HandlingPosition == p.HandlingPosition &&
               MuzzlePosition == p.MuzzlePosition &&
               IsGrenade == p.IsGrenade &&
               MuzzleJump == p.MuzzleJump;
    }

    public override int GetHashCode() {
        return (Id,
                WeaponName,
                FirePower,
                PenetrationPower, 
                FiringInterval, 
                BulletSpeed, 
                MagazineSize, 
                ReloadTime, 
                RecoilAngle, 
                AccuracyMin, 
                AccuracyMax, 
                FullAuto,
                Scope,
                Holding,
                ModelName,
                FireAudio,
                Sound,
                ChangeFireModeWeapon,
                BulletCount,
                HandlingPosition,
                MuzzlePosition,
                IsGrenade,
                MuzzleJump).GetHashCode();
    }

    public static bool operator ==(WeaponSpec lhs, WeaponSpec rhs) => lhs.Equals(rhs);

    public static bool operator !=(WeaponSpec lhs, WeaponSpec rhs) => !(lhs == rhs);

    #endregion

    public static WeaponSpec GetWeaponSpec(short weaponKind)
    {
        try
        {
            return weaponSpecs.Single(s => s.Id == weaponKind);
        }
        catch
        {
            Debug.LogError($"WeaponSpecの読み込みで定義されていないIdを読み込みました(Id:{weaponKind})");
            return weaponSpecs[0];
        }
    }

    public static short NextWeaponId(short id)
    {
        short nextId = (short)(id == (weaponSpecs.Length - 1) ? 0 : id + 1);
        return GetWeaponSpec(nextId).Id;
    }

    public static short PreWeaponId(short id)
    {
        short preId = (short)(id == 0 ? weaponSpecs.Length - 1 : id - 1);
        return GetWeaponSpec(preId).Id;
    }

    public static int WeaponAmmo(short id)
    {
        return GetWeaponSpec(id).MagazineSize;
    }

    public static bool CanChangeFireMode(short kind)
    {
        return GetWeaponSpec(kind).ChangeFireModeWeapon != 0;
    }

    public static bool HasScope(short kind)
    {
        return GetWeaponSpec(kind).Scope != ZoomModeEnum.None;
    }

    public static string GetResourceWeaponModel(short kind)
    {
        WeaponSpec spec = GetWeaponSpec(kind);
        return string.Format(GetResoucePathWeaponFormat, spec.ModelName);
    }

    private static readonly WeaponSpec[] weaponSpecs = new WeaponSpec[]
    {
        new(0, "NONE", 0f, 0f, 0.1f, 100f, 0, 1f, 0f, 0.1f, 5f, false, ZoomModeEnum.None, HoldingMethod.Body, "null", "fire1", 100f, 0, 1, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), false, 0f),
        new(1, "MP5", 40f, 0f, 0.075f, 80f, 30, 2f, 0f, 0.2f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "mp5", "fire1", 100f, 0, 1, new Vector3(0f, -0.0486f, -0.0507f), new Vector3(0f, 0.0133f, 0.2117f), false, 5f),
        new(2, "PSG1", 80f, 0f, 1f, 150f, 5, 4f, 0f, 0f, 5f, false, ZoomModeEnum.Detail, HoldingMethod.Rifle, "psg1", "fire4", 100f, 0, 1, new Vector3(0f, -0.0226f, -0.1085f), new Vector3(0f, 0.0333f, 0.3825f), false, 10f),
        new(3, "M92F", 40f, 0f, 0.1f, 60f, 15, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "m92f", "fire2", 100f, 0, 1, new Vector3(0f, -0.006f, -0.0388f), new Vector3(0f, 0.0443f, 0.0796f), false, 7f),
        new(4, "GLOCK18 SEMI", 40f, 0f, 0.05f, 60f, 19, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "glock18s", "fire2", 100f, 16, 1, new Vector3(0f, -0.0123f, -0.0358f), new Vector3(0f, 0.0377f, 0.0786f), false, 7f),
        new(5, "DESERT EAGLE", 65f, 0f, 0.1f, 60f, 7, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "de", "fire2", 100f, 0, 1, new Vector3(0f, -0.0332f, -0.0573f), new Vector3(0f, 0.0197f, 0.1015f), false, 10f),
        new(6, "MAC10", 40f, 0f, 0.055f, 60f, 30, 1.5f, 0f, 0.3f, 10f, true, ZoomModeEnum.None, HoldingMethod.Pistol, "mac10", "fire1", 100f, 0, 1, new Vector3(0f, 0.0115f, -0.0103f), new Vector3(0f, 0.061f, 0.1021f), false, 7f),
        new(7, "UMP", 50f, 0f, 0.1f, 80f, 25, 2f, 0f, 0.2f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "ump", "fire1", 100f, 0, 1, new Vector3(0f, -0.0332f, -0.0266f), new Vector3(0f, 0.048f, 0.3314f), false, 5f),
        new(8, "P90", 65f, 0f, 0.067f, 100f, 50, 5f, 0f, 0.1f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "p90", "fire1", 100f, 0, 1, new Vector3(0f, -0.0478f, -0.0203f), new Vector3(0f, 0.0141f, 0.2071f), false, 5f),
        new(9, "M4", 65f, 0f, 0.067f, 100f, 30, 2f, 0f, 0.1f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "m4", "fire4", 100f, 0, 1, new Vector3(0f, -0.031f, -0.0789f), new Vector3(0f, 0.0508f, 0.3776f), false, 5f),
        new(10, "AK47", 65f, 0f, 0.1f, 100f, 30, 2f, 0f, 0.1f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "ak47", "fire4", 100f, 0, 1, new Vector3(0f, -0.072f, -0.124f), new Vector3(0f, -0.014f, 0.375f), false, 5f),
        new(11, "AUG", 65f, 0f, 0.071f, 100f, 30, 2f, 0f, 0.1f, 5f, true, ZoomModeEnum.Equal, HoldingMethod.Rifle, "aug", "fire4", 100f, 0, 1, new Vector3(0f, -0.054f, -0.062f), new Vector3(0f, 0.01f, 0.258f), false, 5f),
        new(12, "M249", 65f, 0f, 0.075f, 110f, 100, 5f, 0f, 0.05f, 2f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "m249", "fire4", 100f, 0, 1, new Vector3(0f, -0.0936f, -0.0995f), new Vector3(0f, -0.0014f, 0.3673f), false, 4f),
        new(13, "GRENADE", 200f, 0f, 1.5f, 10f, 1, 0f, 0f, 0.1f, 5f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "grenade", "fire1", 100f, 0, 1, new Vector3(0f, -0.0074f, 0f), new Vector3(0f, 0f, 0f), true, 0f),
        new(14, "MP5SD", 30f, 0f, 0.075f, 70f, 30, 2f, 0f, 0.2f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "mp5sd", "fire1", 100f, 0, 1, new Vector3(0f, -0.0486f, -0.0507f), new Vector3(0f, 0.0106f, 0.2879f), false, 5f),
        new(15, "CASE", 0f, 0f, 0.1f, 0f, 0, 1f, 0f, 0.1f, 5f, false, ZoomModeEnum.None, HoldingMethod.Body, "case", "fire1", 100f, 0, 1, new Vector3(0f, 0.189f, 0f), new Vector3(0f, 0f, 0f), false, 0f),
        new(16, "GLOCK18 FULL", 40f, 0f, 0.05f, 60f, 19, 1.5f, 0f, 0.3f, 10f, true, ZoomModeEnum.None, HoldingMethod.Pistol, "glock18f", "fire2", 100f, 4, 1, new Vector3(0f, -0.0123f, -0.0358f), new Vector3(0f, 0.0377f, 0.0786f), false, 7f),
        new(17, "M1911", 50f, 0f, 0.1f, 60f, 7, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "cg", "fire2", 100f, 0, 1, new Vector3(0f, -0.009f, -0.062f), new Vector3(0f, 0.0429f, 0.0612f), false, 7f),
        new(18, "GLOCK17", 40f, 0f, 0.1f, 60f, 17, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "glock17", "fire2", 100f, 0, 1, new Vector3(0f, -0.0123f, -0.0358f), new Vector3(0f, 0.0377f, 0.0786f), false, 7f),
        new(19, "M1", 30f, 0f, 1f, 50f, 7, 5f, 0f, 0.2f, 5f, false, ZoomModeEnum.None, HoldingMethod.Rifle, "m1", "fire3", 100f, 0, 6, new Vector3(0f, -0.0032f, -0.1558f), new Vector3(0f, 0.0264f, 0.2981f), false, 20f),
        new(20, "FAMAS", 65f, 0f, 0.055f, 100f, 25, 2f, 0f, 0.1f, 5f, true, ZoomModeEnum.None, HoldingMethod.Rifle, "famas", "fire4", 100f, 0, 1, new Vector3(0f, -0.0541f, -0.0373f), new Vector3(0f, 0.0111f, 0.2354f), false, 5f),
        new(21, "MK23", 50f, 0f, 0.1f, 60f, 12, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "mk23", "fire2", 100f, 0, 1, new Vector3(0f, -0.0164f, -0.0507f), new Vector3(0f, 0.0324f, 0.0891f), false, 7f),
        new(22, "MK23 SD", 40f, 0f, 0.1f, 50f, 12, 1.5f, 0f, 0.3f, 10f, false, ZoomModeEnum.None, HoldingMethod.Pistol, "mk23sd", "fire2", 100f, 0, 1, new Vector3(0f, -0.0164f, -0.0507f), new Vector3(0f, 0.0324f, 0.2376f), false, 7f),
    };

    private static readonly string GetResoucePathWeaponFormat = "Assets/Models/Weapon/{0}/{0}.fbx";
}
