using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputTrigger))]
public class TitleManager : MonoBehaviour
{

    private void Start()
    {
        _inputTrigger = GetComponent<InputTrigger>();

        if (MissionDataLoader.ExistsXOps())
        {
            var gameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).Load(DefaultMissionInformation.GetOpening());
        }
        else
        {
            TransitionToMenu();
        }

        Cursor.visible = false;
        //var playerCharacter = gameDataContainer.Characters.SingleOrDefault(c => c.Value.ID == 0);
        //if (_menuCamera != null && !playerCharacter.Equals(default(KeyValuePair<GameObject, CharacterState2>)))
        //{
        //    _menuCamera.Initialize(playerCharacter.Key);
        //}
    }

    private void Update()
    {
        if (_inputTrigger != null)
        {
            if (_inputTrigger.InputEnter("Exit") ||
                _inputTrigger.InputEnter("Fire") ||
                _inputTrigger.InputEnter("Enter"))
            {
                TransitionToMenu();
            }
        }
    }

    public void TransitionToMenu()
    {
        SceneManager.LoadScene("Scene/Menu");
    }

    private InputTrigger _inputTrigger;
}
