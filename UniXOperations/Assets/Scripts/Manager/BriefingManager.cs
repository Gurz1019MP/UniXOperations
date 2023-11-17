using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(InputTrigger))]
public class BriefingManager : MonoBehaviour
{
    public Text missionName;
    public Text Content;
    public Image singlePic;
    public Image doublePic1;
    public Image doublePic2;

    private InputTrigger _inputTrigger;
    private MissionInformation _selectedMissionInformation;

    private void Start()
    {
        _inputTrigger = GetComponent<InputTrigger>();
    }

    void Update()
    {
        if (_inputTrigger.InputEnter("Fire"))
        {
            TransitionToGame();
        }

        if (_inputTrigger.InputEnter("Exit"))
        {
            TransitionToMenu();
        }
    }

    public void Initialize(MissionInformation missionInformation)
    {
        _selectedMissionInformation = missionInformation;

        using (var sr = new StreamReader(missionInformation.MifPath, Encoding.GetEncoding("shift_jis")))
        {
            string pic1name;
            string pic2name;

            if (missionInformation.BlockPath == null || missionInformation.PointPath == null)
            {
                Uri baseUri = new Uri($"{Application.streamingAssetsPath}/XOps/");
                missionInformation.DisplayName = sr.ReadLine();
                missionInformation.Name = sr.ReadLine();
                missionInformation.BlockPath = new Uri(baseUri, sr.ReadLine()).LocalPath;
                missionInformation.PointPath = new Uri(baseUri, sr.ReadLine()).LocalPath;
                missionInformation.Sky = sr.ReadLine();
                sr.ReadLine();
                var komono = new Uri(baseUri, sr.ReadLine()).LocalPath;
                pic1name = new Uri(baseUri, sr.ReadLine()).LocalPath;
                pic2name = new Uri(baseUri, sr.ReadLine()).LocalPath;
            }
            else
            {
                pic1name = sr.ReadLine();
                pic2name = sr.ReadLine();
                missionInformation.Sky = sr.ReadLine();
            }

            string content = sr.ReadToEnd();

            Texture2D pic1 = SetImage(pic1name);
            Texture2D pic2 = SetImage(pic2name);

            if (singlePic != null && doublePic1 != null && doublePic2 != null)
            {
                if (pic1 != null)
                {
                    singlePic.sprite = Sprite.Create(pic1, new Rect(0, 0, pic1.width, pic1.height), Vector2.zero);
                }

                if (pic2 != null)
                {
                    singlePic.gameObject.SetActive(false);
                    doublePic1.gameObject.SetActive(true);
                    doublePic2.gameObject.SetActive(true);

                    doublePic1.sprite = singlePic.sprite;
                    doublePic2.sprite = Sprite.Create(pic2, new Rect(0, 0, pic2.width, pic2.height), Vector2.zero);
                }
            }

            if (Content != null)
            {
                Content.text = content;
            }
        }

        if (missionName != null)
        {
            missionName.text = missionInformation.Name;
        }
    }

    private Texture2D SetImage(string path)
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

    private void TransitionToGame()
    {
        SceneManager.sceneLoaded += GameLoaded;
        SceneManager.LoadScene("Scene/Game");

    }

    private void GameLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= GameLoaded;

        GameCoreManager gameCoreManager = scene.GetRootGameObjects().Single(g => g.name.Equals("GameLogic")).GetComponent<GameCoreManager>();
        gameCoreManager.Initialize(_selectedMissionInformation);
    }

    private void TransitionToMenu()
    {
        SceneManager.LoadScene("Scene/Menu");
    }
}
