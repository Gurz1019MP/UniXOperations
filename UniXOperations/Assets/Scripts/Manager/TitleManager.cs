using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private void Start()
    {
        _playerInputter = new PlayerInputter2();

        if (MissionDataLoader.ExistsXOps())
        {
            new MissionDataLoader(GameObject.Find("Stage")).Load(DefaultMissionInformation.GetOpening());
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

    private PlayerInputter2 _playerInputter;
}
