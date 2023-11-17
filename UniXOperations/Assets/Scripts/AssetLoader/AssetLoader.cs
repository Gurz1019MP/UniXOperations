using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetLoader
{
    public static T LoadAsset<T>(string name) where T : Object
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(name);
#else
        return AssetBundleProvider.LoadAsset<T>(name);
#endif
    }
}
