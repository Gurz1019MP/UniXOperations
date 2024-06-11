using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XOPS;

public class MissionDataLoader
{
    public MissionDataLoader(GameObject stage)
    {
        _stage = stage;
    }

    public static bool ExistsXOps()
    {
        MissionInformation mif = MissionInformation.GetOpening();
        return System.IO.File.Exists(mif.BlockPath) && System.IO.File.Exists(mif.PointPath);
    }

    public GameDataContainer Load(MissionInformation missionInformation)
    {
        //_stage = GameObject.Find("Stage");
        LoadResource();

        return Load(missionInformation.BlockPath, missionInformation.PointPath);
    }

    public GameDataContainer LoadDemo()
    {
        //_stage = GameObject.Find("Stage");
        LoadResource();

        MissionInformation demo = DefaultMissionInformation.GetDemo();

        return Load(demo.BlockPath, demo.PointPath);
    }

    private void LoadResource()
    {
        if (_character != null) return;

        _pointVisual = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabPointVisual);
        _character = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabCharacter);
        _weapon = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabWeaponPickup);
        _path = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabPathContainer);
        _article = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabArticle);
        _missionEvent = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabEventContainer);
    }

    private GameDataContainer Load(string blockPath, string pointPath)
    {
        if (_stage == null) throw new System.InvalidOperationException("ヒエラルキーにゲームオブジェクト\"Stage\"が見つかりません。");
        var gameDataContainer = new GameDataContainer();

        BlockData blockData = bd1loader.LoadBD1(blockPath);

        _stage.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        _stage.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        _stage.GetComponent<MeshRenderer>().materials = blockData.Materials;

        PointData[] pointDatas = pd1loader.LoadPD1(pointPath);

        var characterInformations = CreateCharacterInfomations(pointDatas);
        var paths = CreatePaths(pointDatas, gameDataContainer);
        var characters = CreateCharacters(pointDatas, characterInformations, paths, gameDataContainer);
        var articles = CreateArticles(pointDatas);
        var missionEvents = CreateMissionEvents(pointDatas, gameDataContainer);
        CreateWeapons(pointDatas);

        gameDataContainer.InitPaths(paths);
        gameDataContainer.InitCharacters(characters);
        gameDataContainer.InitArticles(articles);
        gameDataContainer.InitMissionEvents(missionEvents);

        gameDataContainer.InitEventMessage(msgLoader.LoadEventMessage(pointPath));

        return gameDataContainer;
    }

    private CharacterInformations CreateCharacterInfomations(IEnumerable<PointData> pointDatas)
    {
        return new CharacterInformations(pointDatas
            .Where(p => p.Data1 == 4)
            .Select(p => new CharacterInformation(p))
            .ToArray());
    }

    private PathContainer[] CreatePaths(IEnumerable<PointData> pointDatas, GameDataContainer gameDataContainer)
    {
        List<PathContainer> paths = new List<PathContainer>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 == 3 || p.Data1 == 8))
        {
            GameObject instance = Object.Instantiate(_path, pointData.Position * _stage.transform.localScale.x, pointData.Rotation, _stage.transform);
            var pathContainer = instance.GetComponent<PathContainer>();

            if (pointData.Data1 == 3)
            {
                pathContainer.Path = new SinglePath(pointData.Data4, pointData.Data3, (SinglePath.PathKind)pointData.Data2, gameDataContainer);
            }
            else if (pointData.Data1 == 8)
            {
                pathContainer.Path = new RandomPath(pointData.Data4, pointData.Data2, pointData.Data3, gameDataContainer);
            }

            instance.GetComponentInChildren<PointVisualizer>(true).PointData = pointData;

            paths.Add(pathContainer);
        }

        return paths.ToArray();
    }

    private Character[] CreateCharacters(IEnumerable<PointData> pointDatas, CharacterInformations characterInfomations, PathContainer[] paths, GameDataContainer gameDataContainer)
    {
        List<Character> characters = new List<Character>();

        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 1)
            {
                characters.Add(PlaceCharacter(pointData, characterInfomations, paths, gameDataContainer, true));
            }
            else if (pointData.Data1 == 6)
            {
                characters.Add(PlaceCharacter(pointData, characterInfomations, paths, gameDataContainer, false));
            }
        }

        return characters.ToArray();
    }

    private void CreateWeapons(IEnumerable<PointData> pointDatas)
    {
        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 2)
            {
                PlaceWeapon(pointData, false);
            }
            else if (pointData.Data1 == 7)
            {
                PlaceWeapon(pointData, true);
            }
        }
    }

    private ArticleContainer[] CreateArticles(IEnumerable<PointData> pointDatas)
    {
        List<ArticleContainer> articles = new List<ArticleContainer>();

        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 5)
            {
                articles.Add(PlaceArticle(pointData));
            }
        }

        return articles.ToArray();
    }

    private Character PlaceCharacter(PointData pointData, CharacterInformations characterInfomations, PathContainer[] paths, GameDataContainer gameDataContainer, bool equipWeapon2)
    {
        GameObject character = Object.Instantiate(_character, pointData.Position * _stage.transform.localScale.x + Vector3.up * 0.6f, pointData.Rotation);
        Character state = character.GetComponent<Character>();
        CharacterInformation info = characterInfomations.Find(pointData.Data2);

        state.InitCharacterState(pointData, info, equipWeapon2);

        //AIInputterの生成
        var aiInputter = new AIInputter(state, paths.SingleOrDefault(p => p.Path.Id == pointData.Data3), gameDataContainer, CharacterSpec.GetAISkill(info.GetKind()));

        // 各種コンポーネントの初期化
        state.InputterContainer.Initialize(aiInputter);

        return state;
    }

    private void PlaceWeapon(PointData pointData, bool isRandom)
    {
        GameObject instance = Object.Instantiate(_weapon, pointData.Position * _stage.transform.localScale.x, pointData.Rotation * Quaternion.Euler(0, 180, -90));
        WeaponState weapon;

        if (isRandom)
        {
            short kind = Random.Range(0, 2) == 0 ? pointData.Data2 : pointData.Data3;
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(kind);
            weapon = new WeaponState(spec, spec.MagazineSize * 2, spec.MagazineSize);
        }
        else
        {
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(pointData.Data2);
            weapon = new WeaponState(spec, 
                                     pointData.Data3 - spec.MagazineSize, 
                                     pointData.Data3 > spec.MagazineSize ? spec.MagazineSize : pointData.Data3);
        }

        instance.GetComponent<WeaponPickup>().Initialize(weapon);
    }

    private ArticleContainer PlaceArticle(PointData pointData)
    {
        GameObject instance = Object.Instantiate(_article, pointData.Position * _stage.transform.localScale.x, pointData.Rotation);
        ArticleContainer state = instance.GetComponent<ArticleContainer>();

        state.InitArticleSpec(pointData);

        return state;
    }

    private MissionEventContainer[] CreateMissionEvents(PointData[] pointDatas, GameDataContainer gameDataContainer)
    {
        List<MissionEventContainer> missionEvents = new List<MissionEventContainer>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 >= 10 && p.Data1 <= 19))
        {
            GameObject instance = Object.Instantiate(_missionEvent, pointData.Position * _stage.transform.localScale.x, pointData.Rotation, _stage.transform);
            var missionEventContainer = instance.GetComponent<MissionEventContainer>();

            instance.GetComponentInChildren<PointVisualizer>(true).PointData = pointData;

            if (pointData.Data1 == 10)
            {
                missionEventContainer.MissionEvent = new ObjectiveComplateEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 11)
            {
                missionEventContainer.MissionEvent = new MissionFailureEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 12)
            {
                missionEventContainer.MissionEvent = new WaitForDeathEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 13)
            {
                missionEventContainer.MissionEvent = new WaitForArrivalEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 14)
            {
                missionEventContainer.MissionEvent = new ChangeToWalkingEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 15)
            {
                missionEventContainer.MissionEvent = new WaitForDestroyEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 16)
            {
                missionEventContainer.MissionEvent = new WaitForCaseEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 17)
            {
                missionEventContainer.MissionEvent = new WaitForTimerEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 18)
            {
                missionEventContainer.MissionEvent = new ShowMessageEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 19)
            {
                missionEventContainer.MissionEvent = new ChangeTeamEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else
            {
                missionEventContainer.MissionEvent = new DummyEvent(gameDataContainer) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }


            missionEvents.Add(missionEventContainer);
        }

        return missionEvents.ToArray();
    }

    private GameObject _stage;
    private static GameObject _pointVisual;
    private static GameObject _character;
    private static GameObject _weapon;
    private static GameObject _path;
    private static GameObject _article;
    private static GameObject _missionEvent;
}
