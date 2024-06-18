using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MainContainer
{
    private void Start()
    {
        _playerInputter = new InputSystem();

        if (Main.ExistsXOps())
        {
            base.Initialize(MissionInformation.GetOpening());
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

    private InputSystem _playerInputter;
}
