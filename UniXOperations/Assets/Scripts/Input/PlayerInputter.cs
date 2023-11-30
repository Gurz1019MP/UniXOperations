using UnityEngine;

public class PlayerInputter : InputterBase
{
    private PlayerInputter2 _playerInputter;

    public PlayerInputter(GameObject gameObject) : base(gameObject)
    {
        _playerInputter = new PlayerInputter2();
        _playerInputter.Enable();
    }

    protected override void InputUpdate()
    {
        Horizontal = _playerInputter.Player.Horizontal.ReadValue<float>();
        Vertical = _playerInputter.Player.Vertical.ReadValue<float>();
        Fire = _playerInputter.Player.Fire.IsPressed();
        Jump = _playerInputter.Player.Jump.IsPressed();
        MouseX = _playerInputter.Player.MouseX.ReadValue<float>();
        MouseY = _playerInputter.Player.MouseY.ReadValue<float>();
        Walk = _playerInputter.Player.Walk.IsPressed();
        Reload = _playerInputter.Player.Reload.IsPressed();
        DropWeapon = _playerInputter.Player.DropWeapon.IsPressed();
        Zoom = _playerInputter.Player.Zoom.IsPressed();
        FireMode = _playerInputter.Player.FireMode.IsPressed();
        SwitchWeapon = _playerInputter.Player.SwitchWeapon.IsPressed();
        Weapon1 = _playerInputter.Player.Weapon1.IsPressed();
        Weapon2 = _playerInputter.Player.Weapon2.IsPressed();  

        //Horizontal = Input.GetAxis("Horizontal");
        //Vertical = Input.GetAxis("Vertical");
        //Fire = Input.GetAxis("Fire") > 0;
        //Jump = Input.GetAxis("Jump") > 0;
        //MouseX = Input.GetAxis("Mouse X");
        //MouseY = Input.GetAxis("Mouse Y");
        //Walk = Input.GetAxis("Walk") > 0;
        //Reload = Input.GetAxis("Reload") > 0;
        //DropWeapon = Input.GetAxis("Drop Weapon") > 0;
        //Zoom = Input.GetAxis("Zoom") > 0;
        //FireMode = Input.GetAxis("Fire Mode") > 0;
        //SwitchWeapon = Input.GetAxis("Switch Weapon") > 0;
        //Weapon1 = Input.GetAxis("Weapon1") > 0;
        //Weapon2 = Input.GetAxis("Weapon2") > 0;
    }
}
