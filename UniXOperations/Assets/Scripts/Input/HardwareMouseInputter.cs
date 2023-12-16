using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class HardwareMouseInputter : MonoBehaviour
{
    public VirtualMouseInput virtualMouse;

    private InputSystem _inputSystem;

    void Start()
    {
        _inputSystem = new InputSystem();
        _inputSystem.Enable();
    }
    void Update()
    {
        //Vector3 position = virtualMouse.cursorTransform.position;
        //Vector2 inputMouse = new Vector2(_inputSystem.Menu.MouseX.ReadValue<float>(), _inputSystem.Menu.MouseY.ReadValue<float>());
        InputState.Change(virtualMouse.virtualMouse.position, Mouse.current.position.ReadValue());
        Debug.Log($"{_inputSystem.Menu.MouseX.ReadValue<float>()}, {_inputSystem.Menu.MouseY.ReadValue<float>()}");
    }
}
