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
        _characters = CreateCharacters(pointDatas, characterInformations);
        _articles = CreateArticles(pointDatas);
        _eventMessages = new EventMessages(msgLoader.LoadEventMessage(missionInformation.PointPath));
        _missionEvents = CreateMissionEvents(pointDatas, _articles, _characters, _paths, _eventMessages);
        CreateWeapons(pointDatas);

        missionEventManager?.InitEvents(_missionEvents, _characters);

        _characters.CreateCharacterContainer(CreateCharacterContainerAction, characterInformations, _paths);
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

    private Paths CreatePaths(IEnumerable<PointData> pointDatas)
    {
        List<AbstractPath> paths = new List<AbstractPath>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 == 3 || p.Data1 == 8))
        {
            if (pointData.Data1 == 3)
            {
                paths.Add(new SinglePath(pointData));
            }
            else if (pointData.Data1 == 8)
            {
                paths.Add(new RandomPath(pointData));
            }
        }

        return new Paths(paths.ToArray());
    }

    private Characters CreateCharacters(IEnumerable<PointData> pointDatas, CharacterInformations characterInfomations)
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

    private Articles CreateArticles(IEnumerable<PointData> pointDatas)
    {
        List<Article> articles = new List<Article>();

        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 5)
            {
                articles.Add(new Article(pointData));
            }
        }

        return new Articles(articles.ToArray());
    }

    private void PlaceWeapon(PointData pointData, bool isRandom)
    {
        GameObject instance = Object.Instantiate(MissionPrefabProvider.Instance.PrefabWeaponPickup, pointData.Position * _stage.transform.localScale.x, pointData.Rotation * Quaternion.Euler(0, 180, -90));
        Weapon weapon;

        if (isRandom)
        {
            short kind = Random.Range(0, 2) == 0 ? pointData.Data2 : pointData.Data3;
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(kind);
            weapon = new Weapon(spec, WeaponState.EquipWeapon(spec));
        }
        else
        {
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(pointData.Data2);
            weapon = new Weapon(spec, new WeaponState(pointData.Data3 - spec.MagazineSize, pointData.Data3 > spec.MagazineSize ? spec.MagazineSize : pointData.Data3));
        }

        instance.GetComponent<WeaponPickup>().Initialize(weapon);
    }

    private MissionEvents CreateMissionEvents(PointData[] pointDatas, Articles articles, Characters characters, Paths paths, EventMessages eventMessages)
    {
        List<AbstractMissionEvent> missionEvents = new List<AbstractMissionEvent>();

        foreach (PointData pointData in pointDatas.Where(p => p.Data1 >= 10 && p.Data1 <= 19))
        {
            AbstractMissionEvent missionEvent = null;

            if (pointData.Data1 == 10)
            {
                missionEvent = new ObjectiveComplateEvent(pointData);
            }
            else if (pointData.Data1 == 11)
            {
                missionEvent = new MissionFailureEvent(pointData);
            }
            else if (pointData.Data1 == 12)
            {
                missionEvent = new WaitForDeathEvent(pointData, characters);
            }
            else if (pointData.Data1 == 13)
            {
                missionEvent = new WaitForArrivalEvent(pointData, characters);
            }
            else if (pointData.Data1 == 14)
            {
                missionEvent = new ChangeToWalkingEvent(pointData, paths);
            }
            else if (pointData.Data1 == 15)
            {
                missionEvent = new WaitForDestroyEvent(pointData, articles);
            }
            else if (pointData.Data1 == 16)
            {
                missionEvent = new WaitForCaseEvent(pointData, characters);
            }
            else if (pointData.Data1 == 17)
            {
                missionEvent = new WaitForTimerEvent(pointData);
            }
            else if (pointData.Data1 == 18)
            {
                missionEvent = new ShowMessageEvent(pointData, eventMessages);
            }
            else if (pointData.Data1 == 19)
            {
                missionEvent = new ChangeTeamEvent(pointData, characters);
            }
            else
            {
                missionEvent = new DummyEvent(pointData);
            }


            missionEvents.Add(missionEvent);
        }

        return new MissionEvents(missionEvents.ToArray());
    }

    #endregion

    public readonly GameObject _stage;
    public readonly Paths _paths;
    public readonly Characters _characters;
    public readonly Articles _articles;
    public readonly MissionEvents _missionEvents;
    public readonly EventMessages _eventMessages;
}
