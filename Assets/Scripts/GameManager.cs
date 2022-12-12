using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Cinemachine;


using Assets.Scripts.Common;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public static GameManager instance = null;
    [HideInInspector] public bool GamePaused = false;
    [HideInInspector] public Controls controls;

    public GameEnding GameEnding = null;
    public SaveGameManager SaveGameManager = null;
    public Player Player = null;
    public TextMeshProUGUI TestCanvasText = null;
    public GameObject DisplayedMessage = null;
    public GameObject ProTip = null;
    public GameObject SaveText = null;
    public GameObject LoadingText = null;
    public Journal Journal = null;
    public GameObject SightjackOverlay = null;
    public GameObject AccessibleSightjackOverlay = null;
    public GameObject[] StunnedOverlay = null;
    public Camera mainCamera = null;
    public Camera[] additionalCamerasToSetViewDistance = null;
    public CanvasFadeOut canvasFadeOut = null;

    [HideInInspector]public bool IsShitting = false;

    [Tooltip("Reticle shown normally")]
    public GameObject defaultReticle = null;
    [Tooltip("Reticle shown when player is hovering an interactable")]
    public GameObject interactableReticle = null;

    public Canvas playerCanvas = null;

    [Tooltip("How long a message should appear by default")]
    public float defaultMessageLength = 5;
    [Tooltip("How long an item collection message should appear by default")]
    public float defaultItemMessageLength = 3;

    [Tooltip("Music should continue playing while journals, so do not stop sounds in this list")]
    public AudioSource[] MusicList = null;

    private List<AudioSource> audioSourcesToPlayOnUnPause = null;

    private Light[] lightsInScene = null;
    private float[] startingIntensityOfLightsInScene = null;

    private float realTimeAtSceneLoad = -1;

    private SecureSaveFile saveFile = SecureSaveFile.Instance;

    private InputActionMap currentInput = null;
    private InputActionMap previousInput = null;
    
    [HideInInspector] public List<JournalEntry> allJournalEntries = null;

    [HideInInspector] public bool isCameraShaking = false;

    [HideInInspector] public GameMode CurrentGameMode = GameMode.Normal;

    public int NumberOfJournalEntriesInGame => allJournalEntries.Count;

    public enum GameMode
    {
        Normal,
        Story,
        Hardcore,
        DevCommentary
    }

    void Awake()
    {
        instance = this;

        controls = new Controls();

        LoadAllJournalEntries();

        realTimeAtSceneLoad = Time.realtimeSinceStartup;

        lightsInScene = FindObjectsOfType<Light>();
        startingIntensityOfLightsInScene = new float[lightsInScene.Length];

        for (int i = 0; i < lightsInScene.Length; i++)
        {
            startingIntensityOfLightsInScene[i] = lightsInScene[i].intensity;
        }

        //TODO: Figure out where to put this, remove (or change?) resolution options
        //Screen.SetResolution(388, 288, FullScreenMode.FullScreenWindow);
        Screen.SetResolution(480, 360, FullScreenMode.FullScreenWindow);

        if (GetBrightnessSetting() != 1) ApplyBrightness(GetBrightnessSetting());
        ApplyMouseInputProcessors();
        ApplyGamepadInputProcessors();
        ApplyVolume(GetVolume());
        ApplyGraphicsQuality();
        UnPauseGame();

        if (Application.isEditor)
        {
            Debug.Log("GameManager is loaded for the first time, and we are in Editor, so we should reload everything (including checkpoints)");
            SaveGameManager.Instance.ReloadDataFromFile();
            SaveGameManager.Instance.LoadAllData();
        }
        else
        {
        }

        ParseGameMode();
    }

    private void LoadAllJournalEntries()
    {
        allJournalEntries = Resources.LoadAll<JournalEntry>("Journals").ToList();
    }

    private void GetAllJournals()
    {
        if (Player.JournalEntries.Count == allJournalEntries.Count)
        {
            Player.JournalEntries.Clear();
            return;
        }
        
        Player.JournalEntries.Clear();
        foreach (JournalEntry entry in allJournalEntries)
        {
            Player.JournalEntries.Add(entry);
        }

        if (Player.Inventory.Count == InventoryController.itemList.Length)
        {
            Player.Inventory.Clear();
        }
        else
        {
            foreach (InventoryController.Item item in InventoryController.itemList)
            {
                Player.Inventory.Add(item.Name);
            }
        }
    }
    
    public void ParseGameMode()
    {
        if (PlayerPrefs.HasKey("GameMode"))
        {
            switch (PlayerPrefs.GetString("GameMode"))
            {
                case "Story":
                    CurrentGameMode = GameMode.Story;
                    break;
                case "DevCommentary":
                    CurrentGameMode = GameMode.DevCommentary;
                    break;
                case "Hardcore":
                    CurrentGameMode = GameMode.Hardcore;
                    break;
                default:
                    CurrentGameMode = GameMode.Normal;
                    break;
            }
        }
        Debug.Log("Game Mode: " + CurrentGameMode);
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    public void LoadTitleScreen()
    {
        Time.timeScale = 1;
        UpdatePlaytime();
        SceneManager.LoadScene("TitleScreen");
    }

    public void ReachedEnding()
    {
        GameEnding.GameWon();
    }

    public void FellOutOfMap()
    {
        GameEnding.CaughtPlayer(null);
    }

    public void CaughtPlayer(Transform enemyThatCaught)
    {
        GameEnding.CaughtPlayer(enemyThatCaught);
    }

    public void SetSightJacking(bool stunnedSightJack = false, float ambienceDelay = 0, bool dontPlayAmbience = false)
    {
        Player.isSightJacking = true;
        AudioManager.instance.PlaySightJackStart();
        if (!dontPlayAmbience) AudioManager.instance.PlaySightJackAmbience(stunnedSightJack, ambienceDelay);
    }

    public void SwitchInputToPrevious()
    {
        if (previousInput != null) SwitchInput(previousInput);
        else SwitchInput(controls.PlayerControl);
    }

    public void AlertPCSThatNoLongerStunned(EnemyAI enemy)
    {
    }

    public InputActionMap GetInput()
    {
        return currentInput;
    }

    public bool IsPlayerPOVInput()
    {
        return (currentInput.name == "Player Control");
    }

    public void SwitchInput(InputActionMap input)
    {
        if (input == currentInput) return;

        if (currentInput != null)
        {
            currentInput.Disable();
        }
        
        input.Enable();
        previousInput = currentInput;
        currentInput = input;
        
        if (playerCanvas != null) playerCanvas.gameObject.SetActive(input.name == "Player Control");
    }

    public void SetSightJackingVisuals()
    {

    }

    public void SetAnglerEffects()
    {

    }

    public void EndAnglerEffects()
    {

    }

    public void SetOverseerEffects()
    {

        ApplyOverseerGraphicsQuality();
    }

    public void EndOverseerEffects()
    {

        ApplyGraphicsQuality();
    }

    public void EndSightJacking(bool instantStop = false)
    {
        Player.isSightJacking = false;

        EndAnglerEffects();
        EndOverseerEffects();
        if (!instantStop) AudioManager.instance.PlaySightJackStart();
        AudioManager.instance.PauseSightJackAmbience();
        AudioManager.instance.PauseStunnedSightJackAmbience();
    }

    public void DisplayMessage(string message)
    {
        DisplayedMessage.SetActive(true); 
        DisplayedMessage.GetComponent<DisplayedMessage>().DisplayNewMessage(message, defaultMessageLength);
    }

    public void FadeOut()
    {
        canvasFadeOut.StopAllCoroutines();
        StartCoroutine(canvasFadeOut.FadeOut());
    }

    public void FadeIn()
    {
        canvasFadeOut.StopAllCoroutines();
        StartCoroutine(canvasFadeOut.FadeIn());
    }

    public void DisplayMessage(string message, float messageLength)
    {
        DisplayedMessage.SetActive(true);
        DisplayedMessage.GetComponent<DisplayedMessage>().DisplayNewMessage(message, messageLength);
    }

    public void DisplayProTip(string message)
    {
        ProTip.SetActive(true);
        ProTip.GetComponent<DisplayedMessage>().DisplayNewMessage(message, defaultMessageLength);
    }

    public void DisplayProTip(string message, float messageLength)
    {
        ProTip.SetActive(true);
        ProTip.GetComponent<DisplayedMessage>().DisplayNewMessage(message, messageLength);
    }

    public void DisplaySaveText()
    {
        SaveText.SetActive(true);
        SaveText.GetComponent<DisplayedMessage>().DisplayNewMessage("Game Saved...", defaultItemMessageLength);
    }

    public void DisplaySaveTextAfterTime(float time)
    {
        SaveText.SetActive(true);
        StartCoroutine(SaveText.GetComponent<DisplayedMessage>().DisplayNewMessageAfter("Game Saved...", defaultItemMessageLength, time));
    }

    public void DisplayLoadingText()
    {
        LoadingText.SetActive(true);
        LoadingText.GetComponent<DisplayedMessage>().DisplayNewMessage("Loading...", defaultMessageLength);
    }

    public void AddJournalEntry(JournalEntry entry, bool dontOpen = false)
    {
        JournalListUpdated(entry);
        if (dontOpen) return;
        Journal.OpenJournal(entry);
    }

    public void JournalListUpdated(JournalEntry entry)
    {
        if (Player.JournalEntries.Contains(entry)) return; 
        Player.JournalEntries.Add(entry);

    }

    public int JournalListCount()
    {
        try
        {
            return Player.JournalEntries.Count;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to read Player.JournalEntries.Count " + e.ToString());
            return 0;
        }
    }
    
    public void IncreaseDeaths()
    {
        if (!PlayerPrefs.HasKey("PlayerDeaths")) PlayerPrefs.SetInt("PlayerDeaths", 1);
        else PlayerPrefs.SetInt("PlayerDeaths", PlayerPrefs.GetInt("PlayerDeaths") + 1);
    }

    public void UpdatePlaytime()
    {
        if (!PlayerPrefs.HasKey("Playtime")) PlayerPrefs.SetFloat("Playtime", Time.realtimeSinceStartup - realTimeAtSceneLoad);
        else
        {
            float f = PlayerPrefs.GetFloat("Playtime");
            f += (Time.realtimeSinceStartup - realTimeAtSceneLoad);
            PlayerPrefs.SetFloat("Playtime", f);
        }
    }

    //Time when player is on pause screen should not contribute to playtime, so add it to the offset
    public void RemovePausedTime(float time)
    {
        realTimeAtSceneLoad += time;
    }

    public string GetPlaytime()
    {
        if (!PlayerPrefs.HasKey("Playtime")) return "Playtime unavailable";
        TimeSpan ts = TimeSpan.FromSeconds(PlayerPrefs.GetFloat("Playtime"));
        return string.Format("{0}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
    }

    public float GetPlayTimeInSeconds()
    {
        if (PlayerPrefs.HasKey("Playtime")) return PlayerPrefs.GetFloat("Playtime");
        return 99999;
    }

    //For use when loading next scene
    public void DisableTipAndInspect()
    {
        ProTip.SetActive(false);
        DisplayedMessage.SetActive(false);
    }

    public void SetInteractableReticle()
    {
        interactableReticle.SetActive(true);
        defaultReticle.SetActive(false);
    }

    public void SetDefaultReticle()
    {
        interactableReticle.SetActive(false);
        defaultReticle.SetActive(true);
    }

    public bool GetDisplayTipsSetting()
    {
        if (PlayerPrefs.HasKey("DisplayTipsSetting")) return PlayerPrefs.GetInt("DisplayTipsSetting") == 1;
        return DefaultSettings.DISPLAY_TIPS;
    }

    public bool GetVerticalInversionSetting()
    {
        if (PlayerPrefs.HasKey("VerticalInversionSetting")) return PlayerPrefs.GetInt("VerticalInversionSetting") == 1;
        return DefaultSettings.INVERSE_VERTICAL;
    }

    public bool GetMotionBlurSetting()
    {
        return DefaultSettings.MOTION_BLUR;
        if (PlayerPrefs.HasKey("MotionBlurSetting")) return PlayerPrefs.GetInt("MotionBlurSetting") == 1;
        return true;
    }

    public bool GetFilmGrainSetting()
    {
        if (PlayerPrefs.HasKey("FilmGrainSetting")) return PlayerPrefs.GetInt("FilmGrainSetting") == 1;
        return DefaultSettings.FILMGRAIN;
    }

    public float GetBrightnessSetting()
    {
        if (PlayerPrefs.HasKey("BrightnessSetting")) return PlayerPrefs.GetFloat("BrightnessSetting");
        return DefaultSettings.BRIGHTNESS;
    }
    
    public float GetMouseSensitivity()
    {
        if (PlayerPrefs.HasKey("MouseSensitivitySetting")) return PlayerPrefs.GetFloat("MouseSensitivitySetting");
        return DefaultSettings.MOUSE_SENSITIVITY;
    }

    public bool GetMouseInvertY()
    {
        if (PlayerPrefs.HasKey("MouseInvertY")) return PlayerPrefs.GetInt("MouseInvertY") == 1;
        return DefaultSettings.MOUSE_INVERT_Y;
    }

    public bool GetReduceFlashing()
    {
        if (PlayerPrefs.HasKey("ReduceFlashing")) return PlayerPrefs.GetInt("ReduceFlashing") == 1;
        return DefaultSettings.REDUCE_FLASHING;
    }
    
    public string GetGraphicsQuality()
    {
        if (PlayerPrefs.HasKey("GraphicsQuality")) return PlayerPrefs.GetString("GraphicsQuality");
        return DefaultSettings.GRAPHICS_QUALITY;
    }

    public float GetGamepadSensitivity()
    {
        if (PlayerPrefs.HasKey("GamepadSensitivitySetting")) return PlayerPrefs.GetFloat("GamepadSensitivitySetting");
        return DefaultSettings.GAMEPAD_SENSITIVITY;
    }

    public float GetVolume()
    {
        if (PlayerPrefs.HasKey("VolumeLevel")) return PlayerPrefs.GetFloat("VolumeLevel");
        return DefaultSettings.VOLUME;
    }

    public void RestoreDefaultSettings()
    {
        SetMotionBlur(DefaultSettings.MOTION_BLUR);
        SetFilmGrain(DefaultSettings.FILMGRAIN);
        SetBrightness(DefaultSettings.BRIGHTNESS);
        SetMouseSensitivity(DefaultSettings.MOUSE_SENSITIVITY);
        SetGamepadSensitivity(DefaultSettings.GAMEPAD_SENSITIVITY);
        SetVolume(DefaultSettings.VOLUME);
        SetDisplayTipsSetting(DefaultSettings.DISPLAY_TIPS);
        SetVerticalInversionSetting(DefaultSettings.INVERSE_VERTICAL);
        SetGraphicsQuality(DefaultSettings.GRAPHICS_QUALITY);
    }

    public void SetDisplayTipsSetting(bool displayTipValue)
    {
        PlayerPrefs.SetInt("DisplayTipsSetting", displayTipValue ? 1 : 0);
    }

    public void SetReduceFlashing(bool reduceFlashing)
    {
        PlayerPrefs.SetInt("ReduceFlashing", reduceFlashing ? 1 : 0);
    }

    public void ApplyGamepadInputProcessors()
    {
        var processors = "";
        if (GetVerticalInversionSetting())
        {
            processors += "invertVector2(invertY,invertX=false);";
        }

        //        float sensitivity = GetGamepadSensitivity();
        float sensitivity = 8.0f;
        
        // Using the Invariant culture to make numbers always use period as the decimal separator
        FormattableString processor = $"scaleVector2(x={sensitivity},y={sensitivity})";
        processors += FormattableString.Invariant(processor);

        controls.PlayerControl.CameraLook.ApplyBindingOverride(new InputBinding()
        {
            path = "<Gamepad>/rightStick",
            overrideProcessors = processors
        });
    }

    public void ApplyGraphicsQuality()
    {
        string quality = GetGraphicsQuality();

            GameObject[] vcams = GameObject.FindGameObjectsWithTag("VirtualCamera");
            foreach (GameObject vcam in vcams)
            {
                vcam.GetComponent<CinemachineVirtualCamera>().m_Lens.FarClipPlane =
                    GraphicsQuality.Presets[quality].DrawDistance;
            }

        foreach (Camera c in additionalCamerasToSetViewDistance)
        {
            if (c != null && c.gameObject != null) c.farClipPlane = GraphicsQuality.Presets[quality].DrawDistance;
        }
    }

    private void ApplyOverseerGraphicsQuality()
    {
        GameObject[] vcams = GameObject.FindGameObjectsWithTag("VirtualCamera");
        
        // Technically we should only be setting the sightjack camera, but it's easier
        // to just change all of them
        foreach (GameObject vcam in vcams)
        {
            CinemachineVirtualCamera vcamComponent = vcam.GetComponent<CinemachineVirtualCamera>();
            if (vcamComponent.m_Lens.FarClipPlane < GraphicsQuality.OVERSEER_MINIMUM_DRAW_DISTANCE)
            {
                vcamComponent.m_Lens.FarClipPlane = GraphicsQuality.OVERSEER_MINIMUM_DRAW_DISTANCE;
            }
        }       
    }

    public void ApplyMouseInputProcessors()
    {
        var processors = "";
        
        if (GetMouseInvertY())
        {
            processors += "invertVector2(invertY,invertX=false);";
        }
        
        float sensitivity = GetMouseSensitivity();

        // Using the Invariant culture to make numbers always use period as the decimal separator
        FormattableString processor = $"scaleVector2(x={sensitivity},y={sensitivity * 1.2f})";
        processors += FormattableString.Invariant(processor);
        
        controls.PlayerControl.CameraLook.ApplyBindingOverride(new InputBinding()
        {
            path = "<Mouse>/delta",
            overrideProcessors = processors
        });
    }
    public void SetVerticalInversionSetting(bool invertVerticalValue)
    {
        PlayerPrefs.SetInt("VerticalInversionSetting", invertVerticalValue ? 1 : 0);

       ApplyGamepadInputProcessors();
    }

    public void SetMouseInvertY(bool invertVerticalMouse)
    {
        PlayerPrefs.SetInt("MouseInvertY", invertVerticalMouse ? 1 : 0);
        
        ApplyMouseInputProcessors();
    }

    public void SetMouseSensitivity(float mouseSensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivitySetting", mouseSensitivity);
        
        ApplyMouseInputProcessors();
    }

    public void SetGamepadSensitivity(float mouseSensitivity)
    {
        PlayerPrefs.SetFloat("GamepadSensitivitySetting", mouseSensitivity);
        
        ApplyGamepadInputProcessors();
    }

    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("VolumeLevel", volume);
        ApplyVolume(volume);
    }

    public void SetGraphicsQuality(string quality)
    {
        PlayerPrefs.SetString("GraphicsQuality", quality);
        ApplyGraphicsQuality();
    }

    private void ApplyVolume(float vol)
    {
        AudioListener.volume = vol;
    }

    public void SetMotionBlur(bool motionBlurValue)
    {
        PlayerPrefs.SetInt("MotionBlurSetting", motionBlurValue ? 1 : 0);

    }

    public void SetFilmGrain(bool filmGrain)
    {
        PlayerPrefs.SetInt("FilmGrainSetting", filmGrain ? 1 : 0);

    }

    public void SetBrightness(float brightnessValue)
    {
        PlayerPrefs.SetFloat("BrightnessSetting", brightnessValue);
        ApplyBrightness(brightnessValue);
    }

    public void ApplyBrightness(float brightness)
    {
        for (int i = 0; i < lightsInScene.Length; i++)
        {
            if (lightsInScene[i] != null) lightsInScene[i].intensity = startingIntensityOfLightsInScene[i] * brightness;
        }
    }

    private IEnumerator ShakeCameraInternal(float intensity, float duration)
    {
        isCameraShaking = true;

        CinemachineVirtualCamera vcam = (CinemachineVirtualCamera)mainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera;
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        
        yield return new WaitForSeconds(duration);

        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;

        isCameraShaking = false;
    }

    private IEnumerator GongPostProcessingEffects(float duration)
    {
        yield return new WaitForSeconds(duration);

    }

    public void ShakeCamera(float intensity, float duration)
    {
        if (isCameraShaking) return;
        StartCoroutine(ShakeCameraInternal(intensity, duration));
    }

    private CinemachineVirtualCamera[] _cams;

    public void PermanantlyShakeCameras(float intensity)
    {
        if (_cams == null || _cams.Length == 0) _cams = FindObjectsOfType<CinemachineVirtualCamera>();

        foreach (CinemachineVirtualCamera cam in _cams)
        {
            if (cam.name != "Sightjack Camera") cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = intensity;
        }
    }

    public void RingGong(float intensity, float duration)
    {
        ShakeCamera(intensity, duration);
        StartCoroutine(GongPostProcessingEffects(duration));
    }


    public void PauseGame(bool journalPause = false)
    {
        GamePaused = true;
        Time.timeScale = 0;
        
        //Not sure if this is a good solution, generally avoid using 'Find' but could not find any helpful info online shockingly
        audioSourcesToPlayOnUnPause = new List<AudioSource>();
        foreach (AudioSource audioSource in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            if (audioSource.isPlaying)
            {
               if (!journalPause || !MusicListContains(audioSource))
                {
                    if (CurrentGameMode != GameMode.DevCommentary)
                    {
                        audioSourcesToPlayOnUnPause.Add(audioSource);
                        audioSource.Pause();
                    }
                    else
                    {
                        if (!journalPause || audioSource.GetComponent<DevCommentaryInteractable>() == null)
                        {
                            audioSourcesToPlayOnUnPause.Add(audioSource);
                            audioSource.Pause();
                        }
                    }
                }
            }
        }
    }

    private bool MusicListContains(AudioSource aud)
    {
        foreach(AudioSource mus in MusicList)
        {
            if (mus != null && aud == mus) return true;
        }
        return false;
    }

    public void UnPauseGame()
    {
        GamePaused = false;
        Time.timeScale = 1;
        if (audioSourcesToPlayOnUnPause == null) return;
        foreach (AudioSource audioSource in audioSourcesToPlayOnUnPause)
        {
            audioSource.Play();
        }
    }

    private void OnApplicationQuit()
    {
        UpdatePlaytime();
    }
}
