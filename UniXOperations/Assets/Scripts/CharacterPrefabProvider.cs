using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CharacterPrefabProvider
{
    private static CharacterPrefabProvider _instance;

    public GameObject PrefabWeaponPickup;
    public GameObject PrefabDiedCharacter;
    public GameObject PrefabMuzzleFlash;
    public GameObject PrefabBullet;
    public GameObject PrefabGrenade;
    public GameObject PrefabZombieAttack;
    public GameObject PrefabCartridge;
    public Dictionary<string, AudioClip> FireAudioClips;

    public static CharacterPrefabProvider Instance
    {
        get
        {
            _instance ??= new CharacterPrefabProvider();
            return _instance;
        }
    }

    public CharacterPrefabProvider()
    {
        PrefabWeaponPickup = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabWeaponPickup);
        PrefabDiedCharacter = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabDiedCharacter);
        PrefabBullet = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabBullet);
        PrefabGrenade = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabGrenade);
        PrefabZombieAttack = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabZombieAttack);
        PrefabCartridge = AssetLoader.LoadAsset<GameObject>(ConstantsManager.CartridgePrefab);
        PrefabMuzzleFlash = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabMuzzleFlash);
        FireAudioClips = ConstantsManager.FireAudios.ToDictionary(c => c, c => AssetLoader.LoadAsset<AudioClip>(c));
    }
}
