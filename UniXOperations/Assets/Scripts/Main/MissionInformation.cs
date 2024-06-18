using System.IO;
using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;

public struct MissionInformation : IEquatable<MissionInformation>
{
    #region .ctor

    static MissionInformation()
    {
        string blockPath = Application.streamingAssetsPath + "/XOps/data/map0/temp.bd1";
        string pointPath = Application.streamingAssetsPath + "/XOps/data/map0/tr.pd1";
        Fallback = BuildDemoMissionInformation(blockPath, pointPath);
    }

    private MissionInformation(string displayName,
                               string name,
                               string blockPath,
                               string pointPath,
                               string mifPath,
                               string sky,
                               string content,
                               string pic1name,
                               string pic2name)
    {
        DisplayName = displayName;
        Name = name;
        BlockPath = blockPath;
        PointPath = pointPath;
        MifPath = mifPath;
        Sky = sky;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Pic1Name = pic1name;
        Pic2Name = pic2name;
    }

    #endregion

    public string DisplayName { get; }

    public string Name { get; }

    public string BlockPath { get; }

    public string PointPath { get; }

    public string MifPath { get; }

    public string Sky { get; }

    public string Content { get; }

    private string Pic1Name { get; set; }

    private string Pic2Name { get; set; }

    public static MissionInformation Fallback { get; }

    public static MissionInformation BuildMenuMissionInformation(string displayName,
                                                                 string name,
                                                                 string blockPath,
                                                                 string pointPath,
                                                                 string mifPath)
    {
        return new MissionInformation(displayName,
                                      name,
                                      blockPath,
                                      pointPath,
                                      mifPath,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty);
    }

    public static MissionInformation BuildDemoMissionInformation(string blockPath, string pointPath)
    {
        return new MissionInformation(string.Empty,
                                      string.Empty,
                                      blockPath,
                                      pointPath,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty,
                                      string.Empty);
    }

    public static MissionInformation GetOpening()
    {
        string mapPath = $"{Application.streamingAssetsPath}/XOps/data/map10";
        return BuildDemoMissionInformation($"{mapPath}/temp.bd1", $"{mapPath}/op.pd1");
    }

    public static IEnumerable<MissionInformation> GetAddonMissionInformations()
    {
        DirectoryInfo di = new DirectoryInfo(_addonFolder);
        foreach (FileInfo fi in di.GetFiles("*.mif", SearchOption.TopDirectoryOnly))
        {
            yield return BuildMenuMissionInformation(fi.Name,
                                                     fi.Name,
                                                     string.Empty,
                                                     string.Empty,
                                                     fi.FullName);
        }
    }

    public MissionInformation ReadMif()
    {
        using (var sr = new StreamReader(MifPath, Encoding.GetEncoding("shift_jis")))
        {
            string displayName = DisplayName;
            string name = Name;
            string blockPath = BlockPath;
            string pointPath = PointPath;
            string sky = Sky;
            string pic1name;
            string pic2name;

            if (BlockPath == string.Empty || PointPath == string.Empty)
            {
                Uri baseUri = new Uri($"{Application.streamingAssetsPath}/XOps/");
                displayName = sr.ReadLine();
                name = sr.ReadLine();
                blockPath = new Uri(baseUri, sr.ReadLine()).LocalPath;
                pointPath = new Uri(baseUri, sr.ReadLine()).LocalPath;
                sky = sr.ReadLine();
                sr.ReadLine();
                var komono = new Uri(baseUri, sr.ReadLine()).LocalPath;
                pic1name = new Uri(baseUri, sr.ReadLine()).LocalPath;
                pic2name = new Uri(baseUri, sr.ReadLine()).LocalPath;
            }
            else
            {
                pic1name = sr.ReadLine();
                pic2name = sr.ReadLine();
                sky = sr.ReadLine();
            }

            string content = sr.ReadToEnd();

            return new MissionInformation(displayName,
                                          name,
                                          blockPath,
                                          pointPath,
                                          MifPath,
                                          sky,
                                          content,
                                          pic1name,
                                          pic2name);
        }
    }

    public Texture2D GetPic1()
    {
        return GetTexture2D(Pic1Name);
    }

    public Texture2D GetPic2()
    {
        return GetTexture2D(Pic2Name);
    }

    public Material GetSkybox()
    {
        if (!string.IsNullOrEmpty(Sky) && _skyboxMapper.ContainsKey(Sky))
        {
            string assetName = _skyboxMapper[Sky];
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

    #region IEquatable

    public override bool Equals(object obj) => obj is MissionInformation other && this.Equals(other);

    public bool Equals(MissionInformation p)
    {
        return DisplayName == p.DisplayName &&
               Name == p.Name &&
               BlockPath == p.BlockPath &&
               PointPath == p.PointPath &&
               MifPath == p.MifPath &&
               Sky == p.Sky &&
               Content == p.Content &&
               Pic1Name == p.Pic1Name &&
               Pic2Name == p.Pic2Name;
    }

    public override int GetHashCode() => (DisplayName, Name, BlockPath, PointPath, MifPath, Sky, Content, Pic1Name, Pic2Name).GetHashCode();

    public static bool operator ==(MissionInformation lhs, MissionInformation rhs) => lhs.Equals(rhs);

    public static bool operator !=(MissionInformation lhs, MissionInformation rhs) => !(lhs == rhs);

    #endregion

    private static Texture2D GetTexture2D(string path)
    {
        try
        {
            if (path != "!" && Path.GetFileNameWithoutExtension(path) != "!")
            {
                var bmpLoader = new B83.Image.BMP.BMPLoader();
                if (File.Exists(path))
                {
                    if (Path.GetExtension(path).ToLower() == ".bmp")
                    {
                        return bmpLoader.LoadBMP(path).ToTexture2D();
                    }
                    else
                    {
                        Texture2D temp = new Texture2D(2, 2);
                        temp.LoadImage(File.ReadAllBytes(path));
                        return temp;
                    }
                }
                else
                {
                    return bmpLoader.LoadBMP($"{Application.streamingAssetsPath}/XOps/data/briefing/{path}.bmp").ToTexture2D();
                }
            }
            else
            {
                return null;
            }
        }
        catch
        {
            Texture2D temp = new Texture2D(2, 2);
            temp.LoadImage(File.ReadAllBytes(path));
            return temp;
        }
    }

    private static readonly string _addonFolder = $"{Application.streamingAssetsPath}/XOps/addon";

    private static readonly Dictionary<string, string> _skyboxMapper = new Dictionary<string, string>()
    {
        { "0", null },
        { "1", "Assets/Skybox/CasualDay.mat" },
        { "2", "Assets/Skybox/DarkStorm.mat" },
        { "3", "Assets/Skybox/CoriolisNight4k.mat" },
        { "4", "Assets/Skybox/CloudedSunGlow.mat" },
        { "5", "Assets/Skybox/UnearthlyRed.mat" }
    };
}
