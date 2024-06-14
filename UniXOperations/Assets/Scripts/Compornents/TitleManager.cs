using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using XOPS;

public class TitleManager : MonoBehaviour
{
    private void Start()
    {
        _playerInputter = new InputSystem();

        if (GameCore.ExistsXOps())
        {
            _gameCore = new GameCore(GameObject.Find("Stage"), LoadBlock, MissionInformation.GetOpening());
        }
        else
        {
            TransitionToMenu();
        }

        _playerInputter.Menu.Enter.performed += (_) => TransitionToMenu();
        _playerInputter.Menu.Exit.performed += (_) => TransitionToMenu();
        _playerInputter.Enable();

        Cursor.visible = false;
    }

    public void TransitionToMenu()
    {
        _playerInputter.Dispose();
        SceneManager.LoadScene("Scene/Menu");
    }

    private void LoadBlock(BlockData blockData)
    {
        GameObject stage = GameObject.Find("Stage") ?? throw new InvalidOperationException("ヒエラルキーにゲームオブジェクト\"Stage\"が見つかりません。");

        stage.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        stage.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        stage.GetComponent<MeshRenderer>().materials = blockData.Materials;
    }

    private InputSystem _playerInputter;
    private GameCore _gameCore;
}
