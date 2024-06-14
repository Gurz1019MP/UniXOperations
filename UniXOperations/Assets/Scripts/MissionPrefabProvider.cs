using UnityEngine;
public class MissionPrefabProvider
{
    private static MissionPrefabProvider _instance;

    public GameObject PrefabPointVisual { get; }
    public GameObject PrefabCharacter { get; }
    public GameObject PrefabWeaponPickup { get; }
    public GameObject PrefabPathContainer { get; }
    public GameObject PrefabArticle { get; }
    public GameObject PrefabMissionEventContainer { get; }

    public MissionPrefabProvider()
    {
        PrefabPointVisual = AssetLoader.LoadAsset<GameObject>(_prefabPointVisual);
        PrefabCharacter = AssetLoader.LoadAsset<GameObject>(_prefabCharacter);
        PrefabWeaponPickup = AssetLoader.LoadAsset<GameObject>(_prefabWeaponPickup);
        PrefabPathContainer = AssetLoader.LoadAsset<GameObject>(_prefabPathContainer);
        PrefabArticle = AssetLoader.LoadAsset<GameObject>(_prefabArticle);
        PrefabMissionEventContainer = AssetLoader.LoadAsset<GameObject>(_prefabEventContainer);
    }

    public static MissionPrefabProvider Instance
    {
        get
        {
            _instance ??= new MissionPrefabProvider();
            return _instance;
        }
    }

    private static readonly string _prefabPointVisual = "Assets/Prefabs/PointVisual.prefab";

    private static readonly string _prefabCharacter = "Assets/Prefabs/Character.prefab";

    private static readonly string _prefabWeaponPickup = "Assets/Prefabs/WeaponPickup.prefab";

    private static readonly string _prefabPathContainer = "Assets/Prefabs/PathContainer.prefab";

    private static readonly string _prefabArticle = "Assets/Prefabs/Article.prefab";

    private static readonly string _prefabEventContainer = "Assets/Prefabs/EventContainer.prefab";
}
