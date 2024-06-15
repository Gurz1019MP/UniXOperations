using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;
using UnityEngine.TextCore.Text;
using XOPS;

public class GameCore
{
    #region .ctor

    public GameCore(GameObject stage, UnityAction<BlockData> loadBlockAction, MissionInformation missionInformation, UnityAction<Character, CharacterInformations, Paths, Characters> CreateCharacterContainerAction, MissionEventManager missionEventManager = null)
    {
        _stage = stage;

        loadBlockAction.Invoke(bd1loader.LoadBD1(missionInformation.BlockPath));

        PointData[] pointDatas = pd1loader.LoadPD1(missionInformation.PointPath);

        var characterInformations = CreateCharacterInfomations(pointDatas);
        _paths = CreatePaths(pointDatas);
        _characters = CreateCharacters(pointDatas, characterInformations, _paths);
        _articles = CreateArticles(pointDatas);
        _missionEvents = CreateMissionEvents(pointDatas, _articles, _characters, _paths);
        CreateWeapons(pointDatas);

        EventMessages.Initialize(msgLoader.LoadEventMessage(missionInformation.PointPath));
        missionEventManager?.InitEvents(new(_missionEvents.ToArray()), _characters);

        _characters.CreateCharacterContainer(CreateCharacterContainerAction, characterInformations, new Paths(_paths.ToArray()));
    }

    #endregion

    #region Property

    #endregion

    #region Method

    public static bool ExistsXOps()
    {
        MissionInformation mif = MissionInformation.GetOpening();
        return System.IO.File.Exists(mif.BlockPath) && System.IO.File.Exists(mif.PointPath);
    }

    #endregion

    #region PrivateMethod

    private static CharacterInformations CreateCharacterInfomations(IEnumerable<PointData> pointDatas)
    {
        return new CharacterInformations(pointDatas
            .Where(p => p.Data1 == 4)
            .Select(p => new CharacterInformation(p))
            .ToArray());
    }

    private IReadOnlyList<PathContainer> CreatePaths(IEnumerable<PointData> pointDatas)
    {
        List<PathContainer> paths = new List<PathContainer>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 == 3 || p.Data1 == 8))
        {
            GameObject instance = Object.Instantiate(MissionPrefabProvider.Instance.PrefabPathContainer, pointData.Position * _stage.transform.localScale.x, pointData.Rotation, _stage.transform);
            var pathContainer = instance.GetComponent<PathContainer>();

            if (pointData.Data1 == 3)
            {
                pathContainer.Path = new SinglePath(pointData.Data4, pointData.Data3, (SinglePath.PathKind)pointData.Data2);
            }
            else if (pointData.Data1 == 8)
            {
                pathContainer.Path = new RandomPath(pointData.Data4, pointData.Data2, pointData.Data3);
            }

            instance.GetComponentInChildren<PointVisualizer>(true).PointData = pointData;

            paths.Add(pathContainer);
        }

        return paths;
    }

    private Characters CreateCharacters(IEnumerable<PointData> pointDatas, CharacterInformations characterInfomations, IReadOnlyList<PathContainer> paths)
    {
        List<Character> characters = new List<Character>();

        foreach (PointData pointData in pointDatas)
        {
            CharacterInformation info = characterInfomations.Find(pointData.Data2);
            characters.Add(new Character(pointData, info, pointData.Data1 == 1));
        }

        return new Characters(characters.ToArray());
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

    private IReadOnlyList<ArticleContainer> CreateArticles(IEnumerable<PointData> pointDatas)
    {
        List<ArticleContainer> articles = new List<ArticleContainer>();

        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 5)
            {
                articles.Add(PlaceArticle(pointData));
            }
        }

        return articles;
    }

    private void PlaceWeapon(PointData pointData, bool isRandom)
    {
        GameObject instance = Object.Instantiate(MissionPrefabProvider.Instance.PrefabWeaponPickup, pointData.Position * _stage.transform.localScale.x, pointData.Rotation * Quaternion.Euler(0, 180, -90));
        WeaponState weapon;

        if (isRandom)
        {
            short kind = Random.Range(0, 2) == 0 ? pointData.Data2 : pointData.Data3;
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(kind);
            weapon = new WeaponState(spec, spec.MagazineSize * 2, spec.MagazineSize, false, false, false, false);
        }
        else
        {
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(pointData.Data2);
            weapon = new WeaponState(spec,
                                     pointData.Data3 - spec.MagazineSize,
                                     pointData.Data3 > spec.MagazineSize ? spec.MagazineSize : pointData.Data3,
                                     false, false, false, false);
        }

        instance.GetComponent<WeaponPickup>().Initialize(weapon);
    }

    private ArticleContainer PlaceArticle(PointData pointData)
    {
        GameObject instance = Object.Instantiate(MissionPrefabProvider.Instance.PrefabArticle, pointData.Position * _stage.transform.localScale.x, pointData.Rotation);
        ArticleContainer state = instance.GetComponent<ArticleContainer>();

        state.InitArticleSpec(pointData);

        return state;
    }

    private IReadOnlyList<MissionEventContainer> CreateMissionEvents(PointData[] pointDatas, IReadOnlyList<ArticleContainer> articles, Characters characters, IReadOnlyList<PathContainer> paths)
    {
        List<MissionEventContainer> missionEvents = new List<MissionEventContainer>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 >= 10 && p.Data1 <= 19))
        {
            GameObject instance = Object.Instantiate(MissionPrefabProvider.Instance.PrefabMissionEventContainer, pointData.Position * _stage.transform.localScale.x, pointData.Rotation, _stage.transform);
            var missionEventContainer = instance.GetComponent<MissionEventContainer>();

            instance.GetComponentInChildren<PointVisualizer>(true).PointData = pointData;

            if (pointData.Data1 == 10)
            {
                missionEventContainer.MissionEvent = new ObjectiveComplateEvent() { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 11)
            {
                missionEventContainer.MissionEvent = new MissionFailureEvent() { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 12)
            {
                missionEventContainer.MissionEvent = new WaitForDeathEvent(characters) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 13)
            {
                missionEventContainer.MissionEvent = new WaitForArrivalEvent(characters) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 14)
            {
                missionEventContainer.MissionEvent = new ChangeToWalkingEvent(new(paths.ToArray())) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 15)
            {
                missionEventContainer.MissionEvent = new WaitForDestroyEvent(new(articles.ToArray())) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 16)
            {
                missionEventContainer.MissionEvent = new WaitForCaseEvent(characters) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 17)
            {
                missionEventContainer.MissionEvent = new WaitForTimerEvent() { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 18)
            {
                missionEventContainer.MissionEvent = new ShowMessageEvent() { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else if (pointData.Data1 == 19)
            {
                missionEventContainer.MissionEvent = new ChangeTeamEvent(characters) { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }
            else
            {
                missionEventContainer.MissionEvent = new DummyEvent() { Id = pointData.Data4, Data = pointData.Data2, Next = pointData.Data3 };
            }


            missionEvents.Add(missionEventContainer);
        }

        return missionEvents;
    }

    #endregion

    public readonly GameObject _stage;
    public readonly IReadOnlyList<PathContainer> _paths;
    public readonly Characters _characters;
    public readonly IReadOnlyList<ArticleContainer> _articles;
    public readonly IReadOnlyList<MissionEventContainer> _missionEvents;
}
