using UnityEngine.SceneManagement;

public class MenuManager : MainContainer
{
    public MenuCameraController _menuCamera;

    void Start()
    {
        if (Main.ExistsXOps())
        {
            base.Initialize(DefaultMissionInformation.GetDemo());

            var playerCharacter = _gameCore.Characters.Player;
            var playerCharacterContainer = FindCharacterContainer(playerCharacter);
            if (_menuCamera != null && playerCharacterContainer != null)
            {
                _menuCamera.Initialize(playerCharacterContainer);
            }
        }

        SceneManager.LoadScene("Scene/MenuUI", LoadSceneMode.Additive);
    }
}
