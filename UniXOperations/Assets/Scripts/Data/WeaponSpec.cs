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
}
