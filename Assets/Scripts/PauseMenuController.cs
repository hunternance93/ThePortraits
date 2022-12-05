
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("Not needed if this is Title page")]
    public GameObject PauseParent;

    public Selectable PreviousScreenSelected;
    public Toggle DisplayTips;
    public Toggle MotionBlur;
    public Toggle FilmGrain;
    public Toggle InverseVertical;
    public Toggle InverseMouseY;
    public Toggle WindowedMode;
    public Slider BrightnessSlider;
    public Slider MouseSensitivity;
    public Slider GamepadSensitivity;
    public Slider VolumeSlider;
    public Toggle ReduceFlashing;
    public TMP_Dropdown ResolutionDropdown;
    public TMP_Dropdown QualityDropdown;
    public CanvasGroup PausePageCG;
    public CanvasGroup SettingsPageCG;
    public CanvasGroup InventoryPageCG;
    public CanvasGroup HowToPlayCG = null;
    public InventoryController InvCont;
    [Tooltip("If this is the title page instead of pause menu, it will be handled slightly differently")]
    public bool TitleScreen = false;
    public GameObject[] popups = null;
    public TextMeshProUGUI StoryModeText = null;
    public GameObject SwitchGameModeButton = null;
    public CanvasGroup NonPopupSettingsCG = null;
    public Button settingsBackButton = null;
    public TextMeshProUGUI versionLabel = null;
    
    private float timeStampPaused = 0;

    private InputActionMap prePauseInput = null;

    private bool returnToPauseFromInventory = false;

    private List<string> qualitySettings = new List<string>();

    private void Awake()
    {
        if (TitleScreen)
        {
            SetSettingsStates();
        }
        
        if (versionLabel != null)
        {
            versionLabel.text = "Version " + Application.version;
        }

        GameManager.instance.controls.PlayerControl.Pause.performed += context => PauseGame();
        GameManager.instance.controls.None.Pause.performed += context => PauseGame();
        GameManager.instance.controls.UI.Back.performed += Back;

    }

    private void Back(InputAction.CallbackContext obj)
    {
        if (!TitleScreen && InventoryPageCG.interactable) HandleInventoryButton();
        else if ((TitleScreen  && SettingsPageCG.interactable) || (!TitleScreen && (SettingsPageCG.interactable || HowToPlayCG.interactable)))
        {
            foreach (GameObject g in popups) g.SetActive(false);
            SwitchToPausePage();
        }
        else
        {
            UnPauseGame();
        }
    }

    private void HandleInventoryButton()
    {
        if (TitleScreen) return;
        if (InventoryPageCG.interactable)
        {
            SwitchToPausePage();
            if (!returnToPauseFromInventory) UnPauseGame();
        }
        else
        {
            foreach (GameObject g in popups) if (g.activeSelf) return;
            returnToPauseFromInventory = true;
            SwitchToInventoryPage();
        }
    }

    public void SwitchToSettingsPage()
    {
        SettingsPageCG.alpha = 1;
        SettingsPageCG.interactable = true;
        SettingsPageCG.blocksRaycasts = true;
        PausePageCG.alpha = 0;
        PausePageCG.interactable = false;
        PausePageCG.blocksRaycasts = false;
        DisplayTips.Select();
    }

    public void SwitchToInventoryPage()
    {
        SettingsPageCG.alpha = 0;
        SettingsPageCG.interactable = false;
        SettingsPageCG.blocksRaycasts = false;
        if (HowToPlayCG != null)
        {
            HowToPlayCG.alpha = 0;
            HowToPlayCG.interactable = false;
            HowToPlayCG.blocksRaycasts = false;
        }
        InventoryPageCG.alpha = 1;
        InventoryPageCG.interactable = true;
        InventoryPageCG.blocksRaycasts = true;
        PausePageCG.alpha = 0;
        PausePageCG.interactable = false;
        PausePageCG.blocksRaycasts = false;
        InvCont.GenerateInventory();
    }

    public void SwitchToArchive()
    {
        if (TitleScreen) return;
        foreach (GameObject g in popups) if (g.activeSelf) return;
        PausePageCG.alpha = 0;
        PausePageCG.interactable = false;
        PausePageCG.blocksRaycasts = false;
        GameManager.instance.Journal.OpenJournalArchive(this);
    }

    public void SwitchToHowToPlayPage()
    {
        HowToPlayCG.alpha = 1;
        HowToPlayCG.interactable = true;
        HowToPlayCG.blocksRaycasts = true;
        PausePageCG.alpha = 0;
        PausePageCG.interactable = false;
        PausePageCG.blocksRaycasts = false;
    }

    public void SwitchToPausePage()
    {
        SettingsPageCG.alpha = 0;
        SettingsPageCG.interactable = false;
        SettingsPageCG.blocksRaycasts = false;
        if (InventoryPageCG != null)
        {
            InventoryPageCG.alpha = 0;
            InventoryPageCG.interactable = false;
            InventoryPageCG.blocksRaycasts = false;
        }
        PausePageCG.alpha = 1;
        PausePageCG.interactable = true;
        PausePageCG.blocksRaycasts = true;
        if (HowToPlayCG != null)
        {
            HowToPlayCG.alpha = 0;
            HowToPlayCG.interactable = false;
            HowToPlayCG.blocksRaycasts = false;
        }
        if (PreviousScreenSelected != null)
        {
            MenuAudio menuAudio = PreviousScreenSelected.GetComponent<MenuAudio>();
            if (menuAudio != null)
            {
                menuAudio.SuppressNext();
            }
            PreviousScreenSelected.Select();
        }

        if (NonPopupSettingsCG != null)
        {
            NonPopupSettingsCG.interactable = true;
            NonPopupSettingsCG.blocksRaycasts = true;
        }
    }

    public void PauseGame()
    {
        if (TitleScreen || GameManager.instance.Journal.gameObject.activeSelf || GameManager.instance.GameEnding.IsGameOver) return;
        
        GameManager.instance.PauseGame();
        PauseParent.SetActive(true);

        NonPopupSettingsCG.interactable = true;
        NonPopupSettingsCG.blocksRaycasts = true;

        prePauseInput = GameManager.instance.GetInput();
        GameManager.instance.SwitchInput(GameManager.instance.controls.UI.Get());

        timeStampPaused = Time.unscaledTime;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        
        if (PreviousScreenSelected != null)
        {
            MenuAudio menuAudio = PreviousScreenSelected.GetComponent<MenuAudio>();
            if (menuAudio != null)
            {
                menuAudio.SuppressNext();
            }
            PreviousScreenSelected.Select();
        }

        SetSettingsStates();
    }

    public void GoStraightToInventoryPage()
    {
        returnToPauseFromInventory = false;
        PauseGame();
        SwitchToInventoryPage();
    }

    public void SetSettingsStates()
    {
        SetResolutionOptions();
        SetGraphicsQualityOptions();
        
        DisplayTips.isOn = GameManager.instance.GetDisplayTipsSetting();
        MotionBlur.isOn = false;//GameManager.instance.GetMotionBlurSetting();
        FilmGrain.isOn = GameManager.instance.GetFilmGrainSetting();
        ReduceFlashing.isOn = GameManager.instance.GetReduceFlashing();
        
        InverseVertical.isOn = GameManager.instance.GetVerticalInversionSetting();
        InverseMouseY.isOn = GameManager.instance.GetMouseInvertY();
        MouseSensitivity.value = GameManager.instance.GetMouseSensitivity();
        GamepadSensitivity.value = GameManager.instance.GetGamepadSensitivity();
        VolumeSlider.value = GameManager.instance.GetVolume();
        WindowedMode.isOn = !Screen.fullScreen;


        BrightnessSlider.value = GameManager.instance.GetBrightnessSetting();
        if (TitleScreen) GameManager.instance.ApplyBrightness(BrightnessSlider.value);
    }

    public void RestoreDefaultSettings()
    {
        GameManager.instance.RestoreDefaultSettings();
        SetSettingsStates();
    }

    private List<Resolution> GetFilteredResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<Resolution> result = resolutions.Where(resolution => resolution.width >= 1024 && resolution.height >= 768 && resolution.refreshRate == 60).ToList();
        if (result.Count == 0)
        {
            // If there are no 60hz resolutions, this machine might only support 120hz resolutions (e.g. MacBook Pro M1)
            result = resolutions.Where(resolution => resolution.width >= 1024 && resolution.height >= 768 && resolution.refreshRate == 120).ToList();
        }

        return result;
    }

    private void SetResolutionOptions()
    {
        Resolution currentResolution = Screen.currentResolution;

        ResolutionDropdown.options.Clear();
        
        foreach (var res in GetFilteredResolutions())
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData($"{res.width}x{res.height}");
            ResolutionDropdown.options.Add(option);
            
            if (currentResolution.width == res.width && currentResolution.height == res.height)
            {
                ResolutionDropdown.value = ResolutionDropdown.options.Count - 1;
            }
        }
    }

    private void SetGraphicsQualityOptions()
    {
        QualityDropdown.options.Clear();
        qualitySettings.Clear();

        string currentQuality = GameManager.instance.GetGraphicsQuality();

        foreach (KeyValuePair<string, GraphicsQuality.GraphicsQualityPreset> entry in GraphicsQuality.Presets)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(entry.Value.DisplayName);
            QualityDropdown.options.Add(option);
            qualitySettings.Add(entry.Key);
            
            if (currentQuality == entry.Key)
            {
                QualityDropdown.value = QualityDropdown.options.Count - 1;
            }
        }
    }

    public void ApplyWindowedMode()
    {
        Screen.fullScreen = !WindowedMode.isOn;
    }

    public void ApplyResolution()
    {
        List<Resolution> resolutions = GetFilteredResolutions();
        Resolution targetResolution = resolutions[ResolutionDropdown.value];
        Screen.SetResolution(targetResolution.width, targetResolution.height, Screen.fullScreen);
    }

    public void UnPauseGame()
    {
        if (TitleScreen) return;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        SaveSettingsStates();

        PauseParent.SetActive(false);

        foreach(GameObject g in popups)
        {
            g.SetActive(false);
        }

        PausePageCG.interactable = true;

        GameManager.instance.RemovePausedTime(Time.unscaledTime - timeStampPaused);
        timeStampPaused = 0;

        GameManager.instance.UnPauseGame();

        if (prePauseInput != null)
        {
            GameManager.instance.SwitchInput(prePauseInput);
        }
        else
        {
            GameManager.instance.SwitchInputToPrevious();
        }
    }
    
    public void VolumeSliderChanged()
    {
        GameManager.instance.SetVolume(VolumeSlider.value);
    }

    public void BrightnessSliderChanged()
    {
        GameManager.instance.SetBrightness(BrightnessSlider.value);
    }

    public void SaveSettingsStates()
    {
        GameManager.instance.SetDisplayTipsSetting(DisplayTips.isOn);
        //GameManager.instance.SetMotionBlur(MotionBlur.isOn);
        GameManager.instance.SetMotionBlur(false);
        GameManager.instance.SetFilmGrain(FilmGrain.isOn);
        GameManager.instance.SetBrightness(BrightnessSlider.value);
        GameManager.instance.SetVerticalInversionSetting(InverseVertical.isOn);
        GameManager.instance.SetMouseSensitivity(MouseSensitivity.value);
        GameManager.instance.SetMouseInvertY(InverseMouseY.isOn);
        GameManager.instance.SetGamepadSensitivity(GamepadSensitivity.value);
        GameManager.instance.SetVolume(VolumeSlider.value);
        GameManager.instance.SetReduceFlashing(ReduceFlashing.isOn);
        GameManager.instance.SetGraphicsQuality(qualitySettings[QualityDropdown.value]);
    }

    public void SwitchDifficulty()
    {
        string gameMode = GameManager.instance.CurrentGameMode == GameManager.GameMode.Normal ? "Story" : "Normal";
        PlayerPrefs.SetString("GameMode", gameMode);
        if (gameMode == "Story") PlayerPrefs.SetInt("DifficultySwitchedToStory", 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToTitle()
    {
        GameManager.instance.LoadTitleScreen();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
