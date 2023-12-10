using UnityEngine;

public class PlayerInputter : InputterBase
{
    private InputSystem _inputSystem;
    private float _mouseSensitivity;

    public PlayerInputter(GameObject gameObject, InputSystem inputSystem) : base(gameObject)
    {
        _inputSystem = inputSystem;

        if (PlayerPrefs.HasKey(ConstantsManager.MouseSensitivityKey))
        {
            try
            {
                _mouseSensitivity = PlayerPrefs.GetFloat(ConstantsManager.MouseSensitivityKey);
            }
            catch
            {
                Debug.Log("Settings Load Error (MouseSensitivity)");
                _mouseSensitivity = 1.0f;
            }
        }

        _inputSystem.Enable();
    }

    protected override void InputUpdate()
    {
        Horizontal = _inputSystem.Player.Horizontal.ReadValue<float>();
        Vertical = _inputSystem.Player.Vertical.ReadValue<float>();
        Fire = _inputSystem.Player.Fire.IsPressed();
        Jump = _inputSystem.Player.Jump.IsPressed();
        MouseX = _inputSystem.Player.MouseX.ReadValue<float>() * _mouseSensitivity;
        MouseY = _inputSystem.Player.MouseY.ReadValue<float>() * _mouseSensitivity;
        Walk = _inputSystem.Player.Walk.IsPressed();
        Reload = _inputSystem.Player.Reload.IsPressed();
        DropWeapon = _inputSystem.Player.DropWeapon.IsPressed();
        Zoom = _inputSystem.Player.Zoom.IsPressed();
        FireMode = _inputSystem.Player.FireMode.IsPressed();
        SwitchWeapon = _inputSystem.Player.SwitchWeapon.IsPressed();
        Weapon1 = _inputSystem.Player.Weapon1.IsPressed();
        Weapon2 = _inputSystem.Player.Weapon2.IsPressed();  
    }
}
