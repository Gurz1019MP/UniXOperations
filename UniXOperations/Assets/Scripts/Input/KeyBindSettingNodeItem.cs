using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindSettingNodeItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text CurrentKeyText;
    public string ActionName;
    public int ControlIndex;
    public Color MouseOnTextColor;
    public Color DefaultTextColor;

    private PlayerInputter2 _playerInputter;
    private InputAction _action;
    private InputActionRebindingExtensions.RebindingOperation _operation;

    public void SetPlayerInputter(PlayerInputter2 inputter)
    {
        _playerInputter = inputter;

        _action = _playerInputter.FindAction(ActionName);

        CurrentKeyText.color = DefaultTextColor;
        CurrentKeyText.text = _action.GetBindingDisplayString(_action.GetBindingIndexForControl(_action.controls[ControlIndex]));
    }

    public void RebindButton()
    {
        _playerInputter.Disable();

        _operation = _playerInputter.FindAction(ActionName)
            .PerformInteractiveRebinding()
            .WithTargetBinding(_action.GetBindingIndexForControl(_action.controls[ControlIndex]))
            .WithBindingGroup("Keyboard&Mouse")
            .OnMatchWaitForAnother(0.2f)
            .OnCancel(_ => DisposeOperation())
            .OnComplete(_ => DisposeOperation())
            .Start();

        CurrentKeyText.text = _pressKeyText;

        Debug.Log("Rebind Start");
    }

    private void DisposeOperation()
    {
        _operation?.Dispose();
        _operation = null;
        CurrentKeyText.text = _action.GetBindingDisplayString(_action.GetBindingIndexForControl(_action.controls[ControlIndex]));
        _playerInputter.Enable();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CurrentKeyText.color = MouseOnTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CurrentKeyText.color = DefaultTextColor;
    }

    private static readonly string _pressKeyText = "Press Any Key...";
}
