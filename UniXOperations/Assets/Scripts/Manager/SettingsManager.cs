using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public PlayerInputter2 PlayerInputter;
    public Slider FoVSlider;
    public Slider MouseSensitivitySlider;
    public float DefaultFoV;
    public float DefaultMouseSensitivity;
    public GameObject KeyBindSettingNodeContainer;

    private float _foV;
    private float _mouseSensitivity;

    private void Start()
    {
        LoadDefaultSettings();

        if (PlayerPrefs.HasKey(_keyBindingKey))
        {
            try
            {
                PlayerInputter.LoadBindingOverridesFromJson(PlayerPrefs.GetString(_keyBindingKey));
            }
            catch
            {
                Debug.Log("Settings Load Error (KeyBindings)");
            }
        }

        if (PlayerPrefs.HasKey(_fovKey))
        {
            try
            {
                _foV = PlayerPrefs.GetFloat(_fovKey);
            }
            catch
            {
                Debug.Log("Settings Load Error (FoV)");
            }
        }

        if (PlayerPrefs.HasKey(_mouseSensitivityKey))
        {
            try
            {
                _mouseSensitivity = PlayerPrefs.GetFloat(_mouseSensitivityKey);
            }
            catch
            {
                Debug.Log("Settings Load Error (MouseSensitivity)");
            }
        }

        FoVSlider.onValueChanged.AddListener(v => _foV = v);
        MouseSensitivitySlider.onValueChanged.AddListener(v => _mouseSensitivity = v);

        FoVSlider.value = _foV;
        MouseSensitivitySlider.value = _mouseSensitivity;

        foreach (var item in KeyBindSettingNodeContainer.GetComponentsInChildren<KeyBindSettingNodeItem>())
        {
            item.SetPlayerInputter(PlayerInputter);
        }

        PlayerInputter.Menu.Exit.performed += (_) => TransitionToMenu();
        PlayerInputter.Enable();
    }

    public void LoadDefaultSettings()
    {
        PlayerInputter = new PlayerInputter2();
        _foV = DefaultFoV;

        _mouseSensitivity = DefaultMouseSensitivity;
        FoVSlider.value = _foV;
        MouseSensitivitySlider.value = _mouseSensitivity;

        foreach (var item in KeyBindSettingNodeContainer.GetComponentsInChildren<KeyBindSettingNodeItem>())
        {
            item.SetPlayerInputter(PlayerInputter);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetString(_keyBindingKey, PlayerInputter.SaveBindingOverridesAsJson());
        PlayerPrefs.SetFloat(_fovKey, _foV);
        PlayerPrefs.SetFloat(_mouseSensitivityKey, _mouseSensitivity);

        TransitionToMenu();
    }

    private void TransitionToMenu()
    {
        PlayerInputter.Disable();
        SceneManager.LoadScene("Scene/Menu");
    }

    private static readonly string _keyBindingKey = "KeyBinding";
    private static readonly string _fovKey = "FoV";
    private static readonly string _mouseSensitivityKey = "MouseSensitivity";
}
