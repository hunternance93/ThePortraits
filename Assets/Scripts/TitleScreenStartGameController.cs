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
    public GameObject NewGameIntro = null;
    public GameObject CreepyPasta = null;
    public string SceneToLoad = "Indoors";
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

    private const float _fadeLength = 1;

    private CanvasGroup TitleParentCG;

    private void Awake()
    {
        TitleParentCG = TitleParent.GetComponent<CanvasGroup>();

        BlackFadeOutImage.enabled = false;

        GameManager.instance.SwitchInput(GameManager.instance.controls.UI.Get());
        Cursor.lockState = CursorLockMode.None;
    }

    public void HandleNewGame()
    {
        StartCoroutine(LoadNewGame());
    }

    public void HandleCreepyPasta()
    {
        StartCoroutine(PlayCreepyPasta());
    }

    private IEnumerator PlayCreepyPasta()
    {
        TitleParentCG.interactable = false;
        TitleParentCG.blocksRaycasts = false;
        float time = 0;
        CreepyPasta.SetActive(true);
        CanvasGroup cg = CreepyPasta.GetComponent<CanvasGroup>();
        while (time < _fadeLength)
        {
            time += Time.deltaTime;

            TitleParentCG.alpha = Mathf.Lerp(1, 0, time / _fadeLength);
            cg.alpha = Mathf.Lerp(0, 1, time / _fadeLength);

            yield return null;
        }
        TitleParentCG.alpha = 0;
        cg.alpha = 1;
    }

    private IEnumerator LoadNewGame()
    {
        TitleParentCG.interactable = false;
        TitleParentCG.blocksRaycasts = false;
        float time = 0;
        NewGameIntro.SetActive(true);
        CanvasGroup cg = NewGameIntro.GetComponent<CanvasGroup>();
        while (time < _fadeLength)
        {
            time += Time.deltaTime;

            TitleParentCG.alpha = Mathf.Lerp(1, 0, time / _fadeLength);
            cg.alpha = Mathf.Lerp(0, 1, time / _fadeLength);

            yield return null;
        }
        TitleParentCG.alpha = 0;
        cg.alpha = 1;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
