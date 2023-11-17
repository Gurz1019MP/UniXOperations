using System.Collections.Generic;
using UnityEngine;

public static class SkyboxLoader
{
    public static Material GetSkybox(string index)
    {
        if (!string.IsNullOrEmpty(index) && _skyboxMapper.ContainsKey(index))
        {
            string assetName = _skyboxMapper[index];
            if (assetName == null)
            {
                return null;
            }
            else
            {
                return AssetLoader.LoadAsset<Material>(assetName);
            }
        }
        else
        {
            return AssetLoader.LoadAsset<Material>(_skyboxMapper["1"]);
        }
    }

    private static Dictionary<string, string> _skyboxMapper = new Dictionary<string, string>()
    {
        { "0", null },
        { "1", "Assets/Skybox/CasualDay.mat" },
        { "2", "Assets/Skybox/DarkStorm.mat" },
        { "3", "Assets/Skybox/CoriolisNight4k.mat" },
        { "4", "Assets/Skybox/CloudedSunGlow.mat" },
        { "5", "Assets/Skybox/UnearthlyRed.mat" }
    };
}
