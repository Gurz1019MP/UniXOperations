using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CharacterPrefabProvider
{
    private static CharacterPrefabProvider _instance;

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
        PrefabDiedCharacter = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.PrefabDiedCharacter);
        PrefabBullet = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.PrefabBullet);
        PrefabGrenade = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.PrefabGrenade);
        PrefabZombieAttack = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.PrefabZombieAttack);
        PrefabCartridge = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.CartridgePrefab);
        PrefabMuzzleFlash = AssetLoader.LoadAsset<GameObject>(ConstantsProvider.PrefabMuzzleFlash);
        FireAudioClips = ConstantsProvider.FireAudios.ToDictionary(c => c, c => AssetLoader.LoadAsset<AudioClip>(c));
    }
}
