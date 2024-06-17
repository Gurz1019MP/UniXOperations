using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using XOPS;

public class GameCore
{
    #region .ctor

    public GameCore(MissionInformation missionInformation)
    {
        BlockData = bd1loader.LoadBD1(missionInformation.BlockPath);

        PointData[] pointDatas = pd1loader.LoadPD1(missionInformation.PointPath);
        EventMessages = new EventMessages(msgLoader.LoadEventMessage(missionInformation.PointPath));

        CharacterInformations = CreateCharacterInfomations(pointDatas);
        Paths = CreatePaths(pointDatas);
        Characters = CreateCharacters(pointDatas, CharacterInformations);
        Articles = CreateArticles(pointDatas);
        MissionEvents = CreateMissionEvents(pointDatas, Articles, Characters, Paths, EventMessages);
        Weapons = CreateWeapons(pointDatas);
    }

    #endregion

    #region Property

    public BlockData BlockData { get; }
    public CharacterInformations CharacterInformations { get; }
    public Paths Paths { get; }
    public Characters Characters { get; }
    public Articles Articles { get; }
    public MissionEvents MissionEvents { get; }
    public EventMessages EventMessages { get; }
    public Weapons Weapons { get; }

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
            else
            {
                paths.Add(NoPath.Instance);
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
            if (pointData.Data1 == 1)
            {
                characters.Add(new Character(pointData, info, true));
            }
            else if (pointData.Data1 == 6)
            {
                characters.Add(new Character(pointData, info, false));
            }
        }

        return new Characters(characters.ToArray());
    }

    private Weapons CreateWeapons(IEnumerable<PointData> pointDatas)
    {
        List<Weapon> weapons = new List<Weapon>();

        foreach (PointData pointData in pointDatas)
        {
            if (pointData.Data1 == 2)
            {
                weapons.Add(PlaceWeapon(pointData, false));
            }
            else if (pointData.Data1 == 7)
            {
                weapons.Add(PlaceWeapon(pointData, true));
            }
        }

        return new Weapons(weapons.ToArray());
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

    private Weapon PlaceWeapon(PointData pointData, bool isRandom)
    {
        Weapon weapon;

        if (isRandom)
        {
            short kind = Random.Range(0, 2) == 0 ? pointData.Data2 : pointData.Data3;
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(kind);
            weapon = new Weapon(pointData, spec, WeaponState.Build(spec));
        }
        else
        {
            WeaponSpec spec = WeaponSpec.GetWeaponSpec(pointData.Data2);
            weapon = new Weapon(pointData, spec, new WeaponState(pointData.Data3 - spec.MagazineSize, pointData.Data3 > spec.MagazineSize ? spec.MagazineSize : pointData.Data3));
        }

        return weapon;
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
}
