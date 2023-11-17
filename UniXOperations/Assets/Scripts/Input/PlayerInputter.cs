using UnityEngine;

public class PlayerInputter : InputterBase
{
    public PlayerInputter(GameObject gameObject) : base(gameObject)
    {

    }

    protected override void InputUpdate()
    {
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");
        Fire = Input.GetAxis("Fire") > 0;
        Jump = Input.GetAxis("Jump") > 0;
        MouseX = Input.GetAxis("Mouse X");
        MouseY = Input.GetAxis("Mouse Y");
        Walk = Input.GetAxis("Walk") > 0;
        Reload = Input.GetAxis("Reload") > 0;
        DropWeapon = Input.GetAxis("Drop Weapon") > 0;
        Zoom = Input.GetAxis("Zoom") > 0;
        FireMode = Input.GetAxis("Fire Mode") > 0;
        SwitchWeapon = Input.GetAxis("Switch Weapon") > 0;
        Weapon1 = Input.GetAxis("Weapon1") > 0;
        Weapon2 = Input.GetAxis("Weapon2") > 0;
    }
}
