using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public readonly struct CharacterState
{
    private CharacterState(float hitPoint, 
                          Team team, 
                          Weapon weapon1, 
                          Weapon weapon2, 
                          bool isWeapon1, 
                          bool isSwitching, 
                          float armTargetAngle,
                          bool isArmLookAtMode,
                          float accuracyRatio,
                          bool isZoom,
                          Vector3 moveDelta,
                          bool isGround)
    {
        HitPoint = hitPoint;
        Team = team;
        Weapon1 = weapon1;
        Weapon2 = weapon2;
        _isWeapon1 = isWeapon1;
        IsSwitching = isSwitching;
        ArmTargetAngle = armTargetAngle;
        IsArmLookAtMode = isArmLookAtMode;
        AccuracyRatio = accuracyRatio;
        IsZoom = isZoom;
        MoveDelta = moveDelta;
        IsGround = isGround;
    }

    public float HitPoint { get; }

    public Team Team { get; }

    public Weapon Weapon1 { get; }

    public Weapon Weapon2 { get; }

    public bool IsSwitching { get; }

    public float ArmTargetAngle { get; }

    public bool IsArmLookAtMode { get; }

    public float AccuracyRatio { get; }

    public bool IsZoom { get; }

    public Vector3 MoveDelta { get; }

    public bool IsGround { get; }

    public bool IsDied => HitPoint <= 0;

    public Weapon CurrentWeapon => _isWeapon1 ? Weapon1 : Weapon2;

    public Weapon DisableWeapon => _isWeapon1 ? Weapon2 : Weapon1;

    public static CharacterState Initialize(CharacterInformation info, bool equipWeapon2)
    {
        CharacterSpec characterSpec = CharacterSpec.GetCharacterSpec(info.Kind);
        WeaponSpec weapon1 = WeaponSpec.GetWeaponSpec(characterSpec.Weapon1);
        WeaponSpec weapon2 = WeaponSpec.GetWeaponSpec(characterSpec.Weapon2);
        return new CharacterState(CharacterSpec.GetCharacterSpec(info.Kind).Hitpoint,
                                  info.Team,
                                  new Weapon(weapon1, WeaponState.EquipWeapon(weapon1)),
                                  equipWeapon2 ? new Weapon(weapon2, WeaponState.EquipWeapon(weapon2)) : new Weapon(WeaponSpec.None, new WeaponState(0, 0)),
                                  false,
                                  false,
                                  0,
                                  false,
                                  0,
                                  false,
                                  Vector3.zero,
                                  true);
    }

    public CharacterState TakeDamage(float damage)
    {
        return new CharacterState(Mathf.Clamp(HitPoint - damage, 0, HitPoint), Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState Kill()
    {
        return new CharacterState(0, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState ChangeTeam(Team team)
    {
        return new CharacterState(HitPoint, team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState ChangeSwitting(bool isSwitching)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, isSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState SwitchWeapon()
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, !_isWeapon1, false, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState SetArmTargetAngle(float angle)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, angle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState SetIsArmLookAtMode(bool mode)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, mode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState SetCurrentWeapon(Weapon weapon)
    {
        if (_isWeapon1)
        {
            return new CharacterState(HitPoint, Team, weapon, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
        }
        else
        {
            return new CharacterState(HitPoint, Team, Weapon1, weapon, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, IsGround);
        }
    }

    public CharacterState SetAccuracyRatio(float accuracyRatio)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, accuracyRatio, IsZoom, MoveDelta, IsGround);
    }

    public CharacterState SetIsZoom(bool isZoom)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, isZoom, MoveDelta, IsGround);
    }

    public CharacterState SetMoveDelta(Vector3 moveDelta)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, moveDelta, IsGround);
    }

    public CharacterState SetIsGround(bool isGround)
    {
        return new CharacterState(HitPoint, Team, Weapon1, Weapon2, _isWeapon1, IsSwitching, ArmTargetAngle, IsArmLookAtMode, AccuracyRatio, IsZoom, MoveDelta, isGround);
    }

    private readonly bool _isWeapon1;
}
