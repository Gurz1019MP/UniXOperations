using System.Linq;
using UnityEngine;

public static class AssetBundleProvider
{
    private static AssetBundle[] _assetBundles;

    private static AssetBundle[] AssetBundles
    {
        get
        {
            if (_assetBundles == null)
            {
                _assetBundles = System.IO.Directory.GetFiles($"{Application.streamingAssetsPath}/AssetBundle/")
                                                   .Where(f => !System.IO.Path.HasExtension(f))
                                                   .Select(f => AssetBundle.LoadFromFile(f))
                                                   .ToArray();
            }

            return _assetBundles;
        }
    }

    public static T LoadAsset<T>(string name) where T : Object
    {
        if (AssetBundles.Any(sb => sb.Contains(name)))
        {
            return AssetBundles.First(sb => sb.Contains(name)).LoadAsset<T>(name);
        }
        else
        {
            Debug.Log($"アセットがAssetBundleから見つかりませんでした：{name}");
            return null;
        }
    }
}
