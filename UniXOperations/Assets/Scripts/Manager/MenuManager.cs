using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public MenuCameraController _menuCamera;

    void Start()
    {
        if (MissionDataLoader.ExistsXOps())
        {
            var gameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).LoadDemo();
            var playerCharacter = gameDataContainer.Characters.SingleOrDefault(c => c.ID == 0);
            if (_menuCamera != null && playerCharacter != null)
            {
                _menuCamera.Initialize(playerCharacter);
            }
        }

        SceneManager.LoadScene("Scene/MenuUI", LoadSceneMode.Additive);
    }
}
