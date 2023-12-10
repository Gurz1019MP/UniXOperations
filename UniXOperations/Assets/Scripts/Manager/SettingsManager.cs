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

        if (PlayerPrefs.HasKey(ConstantsManager.KeyBindingKey))
        {
            try
            {
                PlayerInputter.LoadBindingOverridesFromJson(PlayerPrefs.GetString(ConstantsManager.KeyBindingKey));
            }
            catch
            {
                Debug.Log("Settings Load Error (KeyBindings)");
            }
        }

        if (PlayerPrefs.HasKey(ConstantsManager.FovKey))
        {
            try
            {
                _foV = PlayerPrefs.GetFloat(ConstantsManager.FovKey);
            }
            catch
            {
                Debug.Log("Settings Load Error (FoV)");
            }
        }

        if (PlayerPrefs.HasKey(ConstantsManager.MouseSensitivityKey))
        {
            try
            {
                _mouseSensitivity = PlayerPrefs.GetFloat(ConstantsManager.MouseSensitivityKey);
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

        Cursor.visible = true;
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
        PlayerPrefs.SetString(ConstantsManager.KeyBindingKey, PlayerInputter.SaveBindingOverridesAsJson());
        PlayerPrefs.SetFloat(ConstantsManager.FovKey, _foV);
        PlayerPrefs.SetFloat(ConstantsManager.MouseSensitivityKey, _mouseSensitivity);

        TransitionToMenu();
    }

    private void TransitionToMenu()
    {
        Cursor.visible = false;

        PlayerInputter.Disable();
        SceneManager.LoadScene("Scene/Menu");
    }
}
