using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using XOPS;

public class MenuManager : MonoBehaviour
{
    public MenuCameraController _menuCamera;

    void Start()
    {
        if (GameCore.ExistsXOps())
        {
            _gameCore = new GameCore(GameObject.Find("Stage"), LoadBlock, DefaultMissionInformation.GetDemo());

            var playerCharacter = new Characters(_gameCore._characters.ToArray()).Player;
            if (_menuCamera != null && playerCharacter != null)
            {
                _menuCamera.Initialize(playerCharacter);
            }
        }

        SceneManager.LoadScene("Scene/MenuUI", LoadSceneMode.Additive);
    }

    private void LoadBlock(BlockData blockData)
    {
        GameObject stage = GameObject.Find("Stage") ?? throw new InvalidOperationException("ヒエラルキーにゲームオブジェクト\"Stage\"が見つかりません。");

        stage.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        stage.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        stage.GetComponent<MeshRenderer>().materials = blockData.Materials;
    }

    private GameCore _gameCore;
}
