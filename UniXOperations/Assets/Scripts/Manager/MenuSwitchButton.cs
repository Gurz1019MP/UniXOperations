﻿using UnityEngine;
using UnityEngine.UI;

public class MenuSwitchButton : MonoBehaviour
{
    public GameObject DefaultMissionList;
    public GameObject AddonMissionList;

    private Text _text;
    private bool _isShowAddons;
    private static string _addonButtonMessage = "AddOn Missions";
    private static string _defaultButtonMessage = "XOps Missions";

    private void Start()
    {
        _text = GetComponentInChildren<Text>();
        ListChange();
    }

    public void ChangeLists()
    {
        _isShowAddons = !_isShowAddons;
        ListChange();
    }

    private void ListChange()
    {
        if (_isShowAddons)
        {
            DefaultMissionList.SetActive(false);
            AddonMissionList.SetActive(true);
            _text.text = _defaultButtonMessage;
        }
        else
        {
            DefaultMissionList.SetActive(true);
            AddonMissionList.SetActive(false);
            _text.text = _addonButtonMessage;
        }
    }
}
