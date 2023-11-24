using System.Collections;
using System.Collections.Generic;

public static class ConstantsManager
{
    #region Prefab

    public static string PrefabCharacter { get; } = "Assets/Prefabs/Character.prefab";

    public static string PrefabWeaponPickup { get; } = "Assets/Prefabs/WeaponPickup.prefab";

    public static string PrefabArticle { get; } = "Assets/Prefabs/Article.prefab";

    public static string PrefabPathContainer { get; } = "Assets/Prefabs/PathContainer.prefab";

    public static string PrefabEventContainer { get; } = "Assets/Prefabs/EventContainer.prefab";

    public static string PrefabBullet { get; } = "Assets/Prefabs/BulletPrefab.prefab";

    public static string PrefabGrenade { get; } = "Assets/Prefabs/GrenadePrefab.prefab";

    public static string PrefabZombieAttack { get; } = "Assets/Prefabs/ZombieAttackPrefab.prefab";

    public static IEnumerable<string> BulletPrefabs = new string[]
    {
        PrefabBullet,
        PrefabGrenade,
        PrefabZombieAttack,
    };

    public static string CartridgePrefab { get; } = "Assets/Prefabs/CartridgePrefab.prefab";

    public static string PrefabDiedCharacter { get; } = "Assets/Prefabs/DiedCharacter.prefab";

    public static string PrefabDiedCamera { get; } = "Assets/Prefabs/DiedCamera.prefab";

    public static string PrefabBrokenArticle { get; } = "Assets/Prefabs/BrokenArticle.prefab";

    public static string PrefabPointVisual { get; } = "Assets/Prefabs/PointVisual.prefab";

    //public static string PrefabCharacter { get; } = "Character";

    //public static string PrefabWeaponPickup { get; } = "WeaponPickup";

    //public static string PrefabArticle { get; } = "Article";

    //public static string PrefabPathContainer { get; } = "PathContainer";

    //public static string PrefabEventContainer { get; } = "EventContainer";

    //public static string PrefabBullet { get; } = "BulletPrefab";

    //public static string PrefabGrenade { get; } = "GrenadePrefab";

    //public static IEnumerable<string> BulletPrefabs = new string[]
    //{
    //    PrefabBullet,
    //    PrefabGrenade,
    //};

    //public static string PrefabDiedCharacter { get; } = "DiedCharacter";

    //public static string PrefabDiedCamera { get; } = "DiedCamera";

    //public static string PrefabBrokenArticle { get; } = "BrokenArticle";

    //public static string PrefabPointVisual { get; } = "PointVisual";

    #endregion

    #region Tag


    public static string TagTarget { get; } = "Target";

    public static string TagBody { get; } = "Body";

    public static string TagUp { get; } = "Up";

    public static string TagArm { get; } = "Arm";

    public static string TagArmHolding { get; } = "Arm_Holding";

    public static string TagLeg { get; } = "Leg";

    public static string TagFpsCameraAnchor { get; } = "FPSCameraAnchor";

    public static string TagTpsCameraAnchor { get; } = "TPSCameraAnchor";

    #endregion

    #region Model

    public static string GetResoucePathCharacterModel(string name) => $"Assets/Models/Character/Built-in/{name}.fbx";

    public static string GetResoucePathCharacterMaterial => $"Assets/Materials/Material.mat";

    public static string GetResoucePathCharacterTexture(string name) => $"Assets/Models/Character/{name}.bmp";

    public static string GetResoucePathArticleModel(string name) => $"Assets/Models/article/{name}.fbx";

    public static string GetResoucePathArticleTexture(string name) => $"Assets/Models/article/{name}.bmp";

    public static string GetResoucePathWeapon(string name) => $"Assets/Models/Weapon/{name}/{name}.fbx";

    #endregion

    #region Audio

    public static string Fire1 { get; } = "Assets/Audio/fire1.wav";

    public static string Fire2 { get; } = "Assets/Audio/fire2.wav";

    public static string Fire3 { get; } = "Assets/Audio/fire3.wav";

    public static string Fire4 { get; } = "Assets/Audio/fire4.wav";

    public static IEnumerable<string> FireAudios = new string[]
    {
        Fire1, Fire2, Fire3, Fire4
    };

    //public static string Fire1 { get; } = "fire1";

    //public static string Fire2 { get; } = "fire2";

    //public static string Fire3 { get; } = "fire3";

    //public static string Fire4 { get; } = "fire4";

    //public static IEnumerable<string> FireAudios = new string[]
    //{
    //    Fire1, Fire2, Fire3, Fire4
    //};

    #endregion
}
