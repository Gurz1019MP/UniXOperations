using UnityEngine;

public class CharacterInputterContainer : MonoBehaviour
{
    private PlayerInputter _playerInputter;
    private AIInputter _aIInputter;

    public InputterBase Inputter { get; private set; }

    public void Initialize(AIInputter aIInputter)
    {
        _aIInputter = aIInputter;
        ChangeInputter();
    }

    public void EnterPlayer(InputSystem inputter)
    {
        if (_playerInputter == null)
        {
            _playerInputter = new PlayerInputter(gameObject, inputter);
        }
        ChangeInputter();
    }

    public void LeavePlayer()
    {
        if (_playerInputter != null)
        {
            _playerInputter = null;
        }
        ChangeInputter();
    }

    private void ChangeInputter()
    {
        if (_playerInputter == null)
        {
            Inputter = _aIInputter;
        }
        else
        {
            Inputter = _playerInputter;
        }
    }
}