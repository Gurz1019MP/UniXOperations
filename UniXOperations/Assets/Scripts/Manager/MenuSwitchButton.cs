using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSwitchButton : MonoBehaviour, IPointerClickHandler
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

    public void OnPointerClick(PointerEventData eventData)
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
