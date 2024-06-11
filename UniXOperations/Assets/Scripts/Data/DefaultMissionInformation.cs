using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct DefaultMissionInformation
{
    public DefaultMissionInformation(byte mapNumber,
                                     string objective = "DEMO",
                                     byte order = 1)
    {
        _mapNumber = mapNumber;
        _objective = objective;
        _order = order;
    }

    public static IEnumerable<MissionInformation> GetMissionInformations()
    {
        foreach (var defaultMission in _defaultMissions)
        {
            string order = defaultMission._order == 1 ? string.Empty : defaultMission._order.ToString();
            string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map{defaultMission._mapNumber}";
            string mifPath = $"{mapPath}/{defaultMission._objective.ToLower()}{order}.txt";

            yield return MissionInformation.BuildMenuMissionInformation(
                $"{_mapNameMapper[defaultMission._mapNumber]}_{defaultMission._objective}{order}",
                $"{_mapNameMapper[defaultMission._mapNumber]} {_objectiveMapper[defaultMission._objective]}{order}",
                $"{mapPath}/temp.bd1",
                $"{mapPath}/{defaultMission._objective.ToLower()}{order}.pd1",
                mifPath);
        }
    }

    public static MissionInformation GetDemo()
    {
        MissionInformation[] demoMissionInformations = _demos.Select(d =>
        {
            string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map{d._mapNumber}";
            return MissionInformation.BuildDemoMissionInformation($"{mapPath}/temp.bd1", $"{mapPath}/demo.pd1");
        }).ToArray();

        return demoMissionInformations[Random.Range(0, demoMissionInformations.Length)];
    }

    private static readonly DefaultMissionInformation[] _defaultMissions = new DefaultMissionInformation[]
    {
        new(0, "TR", 1),
        new(5, "EXT", 1),
        new(1, "EXT", 1),
        new(2, "EXT", 1),
        new(4, "EXT", 1),
        new(7, "EXT", 1),
        new(9, "EXT", 1),
        new(6, "EXT", 1),
        new(13, "EXT", 1),
        new(14, "EXT", 1),
        new(8, "EXT", 1),
        new(5, "EXT", 2),
        new(12, "EXT", 1),
        new(8, "DEF", 1),
        new(3, "EXT", 1),
        new(8, "DEF", 2),
        new(7, "KT", 1),
        new(14, "RE", 1),
        new(16, "CAP", 1),
        new(4, "DE", 1),
        new(14, "CAP", 1),
        new(3, "KT", 1),
        new(2, "RE", 1),
        new(5, "DEF", 1),
        new(12, "EXT", 2),
        new(16, "DEF", 1),
        new(8, "KT", 1),
        new(10, "KT", 1),
        new(6, "KT", 1),
        new(7, "DEF", 1),
        new(8, "KT", 2),
        new(15, "DEF", 1),
        new(8, "CAP", 1),
        new(10, "EXT", 1),
        new(12, "ESC", 1),
        new(13, "CAP", 1),
        new(7, "DEF", 2),
        new(14, "DE", 1),
        new(8, "KT", 3),
        new(9, "EXT", 2),
        new(16, "EXT", 1),
        new(2, "DEF", 1),
        new(1, "KT", 1),
        new(13, "ESC", 1),
        new(10, "EXT", 2),
        new(4, "ESC", 1),
        new(3, "DEF", 1),
        new(15, "KT", 1),
        new(1, "DEF", 1),
        new(15, "KT", 2),
        new(4, "CAP", 1),
        new(12, "KT", 1),
        new(15, "RE", 1),
        new(8, "KT", 4),
        new(5, "EXT", 3),
        new(11, "EXT", 1),
        new(11, "EXT", 2),
        new(11, "DE", 1),
    };

    private static readonly DefaultMissionInformation[] _demos = new DefaultMissionInformation[]
    {
        new(2),
        new(8),
        new(7),
        new(4),
        new(5),
        new(16),
    };

    private static readonly Dictionary<byte, string> _mapNameMapper = new Dictionary<byte, string>()
    {
        { 0, "TRAINING YARD" },
        { 1, "BUILDING" },
        { 2, "SNOW BASE" },
        { 3, "DTOWN" },
        { 4, "MBASE" },
        { 5, "UNDERGROUND" },
        { 6, "STATION" },
        { 7, "WAREHOUSE" },
        { 8, "URBAN" },
        { 9, "DUEL" },
        { 10, "ALLEY" },
        { 11, "SCHOOL" },
        { 12, "TUNNEL" },
        { 13, "MAZE" },
        { 14, "RUINS" },
        { 15, "OFFICE" },
        { 16, "RELIC" },
    };

    private static readonly Dictionary<string, string> _objectiveMapper = new Dictionary<string, string>()
    {
        { "CAP", "capture" },
        { "DE", "destroy" },
        { "DEF", "defend target" },
        { "ESC", "escape" },
        { "EXT", "extermination" },
        { "KT", "kill the target" },
        { "RE", "release" },
        { "TR", "training" },
    };

    private readonly byte _mapNumber;
    private readonly string _objective;
    private readonly byte _order;
}
