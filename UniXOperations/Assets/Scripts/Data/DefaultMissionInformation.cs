using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using B83.Image.BMP;

class DefaultMissionInformation
{
    public byte MapNumber { get; set; }

    public string Objective { get; set; }

    public byte Order { get; set; }

    public static IEnumerable<MissionInformation> GetMissionInformation()
    {
        foreach (var defaultMission in _defaultMissions)
        {
            string order = defaultMission.Order == 1 ? string.Empty : defaultMission.Order.ToString();
            string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map{defaultMission.MapNumber}";
            string mifPath = $"{mapPath}/{defaultMission.Objective.ToLower()}{order}.txt";

            yield return new MissionInformation()
            {
                DisplayName = $"{_mapNameMapper[defaultMission.MapNumber]}_{defaultMission.Objective}{order}",
                Name = $"{_mapNameMapper[defaultMission.MapNumber]} {_objectiveMapper[defaultMission.Objective]}{order}",
                BlockPath = $"{mapPath}/temp.bd1",
                PointPath = $"{mapPath}/{defaultMission.Objective.ToLower()}{order}.pd1",
                MifPath = mifPath,
            };
        }
    }

    public static IEnumerable<MissionInformation> GetDemo()
    {
        foreach (var demo in _demos)
        {
            string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map{demo.MapNumber}";
            yield return new MissionInformation()
            {
                BlockPath = $"{mapPath}/temp.bd1",
                PointPath = $"{mapPath}/demo.pd1",
            };
        }
    }

    public static MissionInformation GetOpening()
    {
        string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map10";
        return new MissionInformation()
        {
            BlockPath = $"{mapPath}/temp.bd1",
            PointPath = $"{mapPath}/op.pd1",
        };
    }

    private static readonly DefaultMissionInformation[] _defaultMissions = new DefaultMissionInformation[]
    {
        new DefaultMissionInformation(){ MapNumber = 0, Objective = "TR", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 5, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 1, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 2, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 4, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 7, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 9, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 6, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 13, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 14, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 5, Objective = "EXT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 12, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 3, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "DEF", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 7, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 14, Objective = "RE", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 16, Objective = "CAP", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 4, Objective = "DE", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 14, Objective = "CAP", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 3, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 2, Objective = "RE", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 5, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 12, Objective = "EXT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 16, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 10, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 6, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 7, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "KT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 15, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "CAP", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 10, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 12, Objective = "ESC", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 13, Objective = "CAP", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 7, Objective = "DEF", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 14, Objective = "DE", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "KT", Order = 3 },
        new DefaultMissionInformation(){ MapNumber = 9, Objective = "EXT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 16, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 2, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 1, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 13, Objective = "ESC", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 10, Objective = "EXT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 4, Objective = "ESC", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 3, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 15, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 1, Objective = "DEF", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 15, Objective = "KT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 4, Objective = "CAP", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 12, Objective = "KT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 15, Objective = "RE", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 8, Objective = "KT", Order = 4 },
        new DefaultMissionInformation(){ MapNumber = 5, Objective = "EXT", Order = 3 },
        new DefaultMissionInformation(){ MapNumber = 11, Objective = "EXT", Order = 1 },
        new DefaultMissionInformation(){ MapNumber = 11, Objective = "EXT", Order = 2 },
        new DefaultMissionInformation(){ MapNumber = 11, Objective = "DE", Order = 1 },
    };

    private static readonly DefaultMissionInformation[] _demos = new DefaultMissionInformation[]
    {
        new DefaultMissionInformation(){ MapNumber = 2 },
        new DefaultMissionInformation(){ MapNumber = 8 },
        new DefaultMissionInformation(){ MapNumber = 7 },
        new DefaultMissionInformation(){ MapNumber = 4 },
        new DefaultMissionInformation(){ MapNumber = 5 },
        new DefaultMissionInformation(){ MapNumber = 16 },
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
}
