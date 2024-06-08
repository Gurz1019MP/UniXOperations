using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

public static class AddonMissionInformationLoader
{
    private static string _addonFolder = $"{Application.streamingAssetsPath}/XOps/addon";

    public static IEnumerable<MissionInformation> GetMissionInformation()
    {
        DirectoryInfo di = new DirectoryInfo(_addonFolder);
        foreach (FileInfo fi in di.GetFiles("*.mif", SearchOption.TopDirectoryOnly))
        {
            yield return new MissionInformation()
            {
                DisplayName = fi.Name,
                MifPath = fi.FullName,
            };
        }
    }
}
