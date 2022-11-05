using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Assets.Scripts.Common;
using UnityEngine.InputSystem;

public class TitleScreenStartGameController : MonoBehaviour
{
    public GameObject Popup = null;
    public Button NewGameButton = null;
    public Button LoadGameButton = null;
    public GameObject TitleParent = null;
    public GameObject LoadingText = null;
    public CanvasGroup NewGameIntroCG = null;
    public string SceneToLoad = "Level1V2";
    public AudioSource HeartBeat = null;
    public Image BlackFadeOutImage = null;
    public GameObject IntroChildObject = null;
    public CanvasGroup NewGameModeSelectPopup = null;
    public CanvasGroup MainMenuCG = null;
    public Button HardcoreNewGameButton = null;
    public Button DevCommentaryNewGameButton = null;
    public AudioSource waterSound = null;

    public TextMeshProUGUI StoryModeDescription = null;
    public TextMeshProUGUI CommentaryModeDescription = null;

    private static SecureSaveFile saveFile = SecureSaveFile.Instance;

    private const float _fadeLength = 1;

    private CanvasGroup TitleParentCG;

    private string _inputHistory = "";

    private void Awake()
    {
        TitleParentCG = TitleParent.GetComponent<CanvasGroup>();

        if (!saveFile.DoesFileExistOnDisk() || GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore)
        {
            LoadGameButton.interactable = false;
        }
        BlackFadeOutImage.enabled = false;

        GameManager.instance.SwitchInput(GameManager.instance.controls.UI.Get());
        Cursor.lockState = CursorLockMode.None;
        
#if STEAMWORKS_ATAMA
        StoryModeDescription.text = StoryModeDescription.text + " Some achievements are not available in this mode.";
        CommentaryModeDescription.text = CommentaryModeDescription.text + " Some achievements are not available in this mode.";
#endif
    }

    private void Update()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            _inputHistory += "U";
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            _inputHistory += "D";
        }
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            _inputHistory += "L";
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            _inputHistory += "R";
        }
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            _inputHistory += "B";
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            _inputHistory += "A";
            if (_inputHistory.Contains("UUDDLRLRBA") && (!PlayerPrefs.HasKey("GameCompleted") || PlayerPrefs.GetInt("GameCompleted") != 1))
            {
                HeartBeat.Play();
                PlayerPrefs.SetInt("GameCompleted", 1);
            }
        }
    }

    public void HandleNewGame()
    {
        if (saveFile.DoesFileExistOnDisk())
        {
            if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore)
            {
                DeleteDataForNewGame();
                OpenModePopup();
            }
            else
            {
                Popup.SetActive(true);
                MainMenuCG.interactable = false;
            }
        }
        else
        {
            DeleteDataForNewGame();
            OpenModePopup();
        }
    }

    public void ImmediatelyLoadNewGame()
    {
        HandleLoadSave(false);
    }

    public void LoadNewGameOfMode(string mode)
    {
        StartCoroutine(LoadNewGame(mode));
    }

    private IEnumerator LoadNewGame(string mode = "Normal")
    {
        PlayerPrefs.SetString("GameMode", mode);
        if (mode == "Story") PlayerPrefs.SetInt("DifficultySwitchedToStory", 1);
        GameManager.instance.ParseGameMode();
        HeartBeat.Play();
        waterSound.Stop();
        TitleParentCG.interactable = false;
        TitleParentCG.blocksRaycasts = false;
        float time = 0;
        NewGameIntroCG.gameObject.SetActive(true);
        NewGameIntroCG.interactable = true;
        NewGameIntroCG.blocksRaycasts = true;
        while (time < _fadeLength)
        {
            time += Time.deltaTime;

            TitleParentCG.alpha = Mathf.Lerp(1, 0, time / _fadeLength);
            NewGameIntroCG.alpha = Mathf.Lerp(0, 1, time / _fadeLength);

            yield return null;
        }
        TitleParentCG.alpha = 0;
        NewGameIntroCG.alpha = 1;
    }

    public void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        saveFile.DeleteFileFromDisk();
        saveFile.DeleteAll();
    }

    public void DeleteDataForNewGame()
    {
        saveFile.DeleteFileFromDisk();
        // Also delete hardcore mode save file
        saveFile.DeleteFileFromDisk(true);
        saveFile.DeleteAll();
        PlayerPrefs.DeleteKey("HasPlayedRooftopMusic");
        PlayerPrefs.DeleteKey("PlayerDeaths");
        PlayerPrefs.DeleteKey("Playtime");
        PlayerPrefs.DeleteKey("GameMode");
        PlayerPrefs.DeleteKey("DifficultySwitchedToStory"); 
    }

    public void OverwriteSave()
    {
        DeleteDataForNewGame();
        HandleNewGame();
    }

    public void OpenModePopup()
    {
        MainMenuCG.interactable = false;
        MainMenuCG.blocksRaycasts = false;
        MainMenuCG.alpha = 0;
        NewGameModeSelectPopup.interactable = true;
        NewGameModeSelectPopup.blocksRaycasts = true;
        NewGameModeSelectPopup.alpha = 1;
        if (PlayerPrefs.HasKey("GameCompleted") && PlayerPrefs.GetInt("GameCompleted") == 1)
        {
            HardcoreNewGameButton.interactable = true;
            DevCommentaryNewGameButton.interactable = true;
        }
    }

    public void CloseModePopup()
    {
        MainMenuCG.interactable = true;
        MainMenuCG.blocksRaycasts = true;
        MainMenuCG.alpha = 1;
        NewGameModeSelectPopup.interactable = false;
        NewGameModeSelectPopup.blocksRaycasts = false;
        NewGameModeSelectPopup.alpha = 0;
    }

    private IEnumerator FadeToBlackThenLoad()
    {
        if (!SecureSaveFile.Instance.DoesFileExistOnDisk())
        {
            SecureSaveFile.Instance.SetString(SaveFileConstants.CurrentScene, "Level1V2");
            SecureSaveFile.Instance.SaveToDisk();
        }
        TitleParentCG.interactable = false;
        TitleParentCG.blocksRaycasts = false;

        // ensure the image is all black and transparent
        BlackFadeOutImage.enabled = true;
        BlackFadeOutImage.color = new Color(0, 0, 0, 0);
        float time = 0;
        float fadeLength = HeartBeat.clip.length / 3; // The actual heartbeat is about a third of the length of the track
        while (time < fadeLength)
        {
            time += Time.unscaledDeltaTime;

            BlackFadeOutImage.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, time*2 / fadeLength)); // scale to black at double the speed
            yield return null;
        }

        SaveGameManager.Instance.LoadGame();
    }

    public void HandleLoadSave(bool dontPlayHeartbeat = false)
    {
        if (!dontPlayHeartbeat) HeartBeat.Play();

        StartCoroutine(FadeToBlackThenLoad());
    }

    //Temp to be able to access Manor scene in builds before it is connected to overall game
    public void HandleLoadManor()
    {
        TitleParent.SetActive(false);
        LoadingText.SetActive(true);

        HeartBeat.Play();

        SceneManager.LoadScene("Manor");
    }

    public void HandleLoadDemo2()
    {
        DeleteAllData();
        SecureSaveFile.Instance.SetString(SaveFileConstants.CurrentCheckpoint, "ContinueFromDemo1CheckPoint");
        SecureSaveFile.Instance.SetString(SaveFileConstants.CurrentScene, "Manor");
        SecureSaveFile.Instance.SaveToDisk();
        HandleLoadSave();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
