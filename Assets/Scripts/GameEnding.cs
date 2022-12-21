using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Assets.Scripts.Common;
using System;

//TODO: Cleanup script as it contains a lot of outdated shit

public class GameEnding : MonoBehaviour
{
    public CanvasGroup GameOverOptionsCanvasGroup = null;
    public CanvasGroup GoodEndingCanvasGroup = null;
    public TextMeshProUGUI GoodEndingJournalCount = null;
    public TextMeshProUGUI PlaytimeText = null;
    public GameObject GameOverOptions = null;
    public GameObject LoadingText = null;
    public CinemachineVirtualCamera deathCamera = null;
    public GameObject SightjackOverlay = null;
    public GameObject BlinkObject = null;
    public Selectable initialSelected = null;
    public AudioSource gameEndingAudio = null;
    public GameObject StoryModeGameOverFlashObjectPrefab = null;
    public GameObject CanvasObj;

    private AsyncOperation reloadScene = null;
    private IEnumerator asyncReloadRoutine = null;

    private const float _gameOverFadeInSpeed = 3f;

    private Flash storyFlash = null;

    [HideInInspector]
    public bool IsGameOver = false;

    private void Start()
    {
        if(GameManager.instance.CurrentGameMode == GameManager.GameMode.Story || GameManager.instance.CurrentGameMode == GameManager.GameMode.DevCommentary)
        {
            GameObject flash = Instantiate(StoryModeGameOverFlashObjectPrefab);
            Transform before = flash.transform;
            flash.transform.SetParent(CanvasObj.transform);
            flash.transform.SetPositionAndRotation(before.position, before.rotation);
            RectTransform rectT = flash.GetComponent<RectTransform>();
            rectT.localScale = new Vector3(1, 1, 1);
            rectT.offsetMin = new Vector2(1, 1);
            rectT.offsetMax = new Vector2(1, 1);
            storyFlash = flash.GetComponent<Flash>();
        }
    }

    public void CaughtPlayer(Transform enemyThatCaught)
    {
        if (!IsGameOver)
        {
            IsGameOver = true;
            StartCoroutine(GameOverCoroutine(enemyThatCaught));
        }
    }

    public void GameWon() 
    {
        if (IsGameOver) return;
        IsGameOver = true;
        StartCoroutine(GameWonCoroutine());
    }

    private IEnumerator GameOverCoroutine(Transform enemyThatCaught)
    {
        GameManager.instance.PauseGame();
        AudioManager.instance.PlayDeath();
        AudioManager.instance.StopPlayerMovementAudio();
        //GameManager.instance.Player.PCS.SetPlayerMeshRenderers(false);
        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore) initialSelected.gameObject.SetActive(false);
        GameOverOptionsCanvasGroup.interactable = true;
        GameOverOptionsCanvasGroup.blocksRaycasts = true;
        if (initialSelected != null && initialSelected.gameObject.activeSelf)
        {
            initialSelected.Select();
        }
        Cursor.lockState = CursorLockMode.None;

        GameManager.instance.UpdatePlaytime();
        GameManager.instance.IncreaseDeaths();
        
        if (enemyThatCaught != null)
        {
            deathCamera.LookAt = enemyThatCaught;
            deathCamera.Priority = 999;
        }

        yield return null;
        KaedeVoiceManager.instance.TriggerScream();

        float time = 0;
        yield return null;
        AudioManager.instance.StopPlayerMovementAudio();
        time += Time.deltaTime;
        while (time < _gameOverFadeInSpeed)
        {
            time += Time.unscaledDeltaTime;
            GameOverOptionsCanvasGroup.alpha = Mathf.Lerp(0, 1, time / _gameOverFadeInSpeed);
            yield return null;
        }
        GameOverOptionsCanvasGroup.alpha = 1;

        SceneManager.LoadScene("Credits");
    }

    private IEnumerator StoryModeDeathCoroutine(Transform enemyThatCaught)
    {
        GameManager.instance.PauseGame();
        AudioManager.instance.StopDeathSounds();
        AudioManager.instance.PlayDeath();
        
        AudioManager.instance.StopPlayerMovementAudio();
        GameManager.instance.IncreaseDeaths();

        deathCamera.LookAt = enemyThatCaught;
        deathCamera.Priority = 999;

        yield return null;
        KaedeVoiceManager.instance.TriggerScream();
        yield return new WaitForSecondsRealtime(_gameOverFadeInSpeed);
        yield return storyFlash.FlashScreen(1);
        EnemyAI enemy = enemyThatCaught.GetComponentInParent<EnemyAI>();
        enemy.SetDisabledStory();
        deathCamera.Priority = -999;
        deathCamera.LookAt = null;
        if (enemy.GetState() == EnemyAI.EnemyState.Chasing) GameManager.instance.Player.EnemyAggroCount--;
        enemy.transform.position = new Vector3(0, -500, 0);

        IsGameOver = false;
        GameManager.instance.UnPauseGame();

        //Do this a second time in case any effects caused the sightjack audio to play again
        //GameManager.instance.Player.PCS.InstantlyEndSightjacking();
    }

    private IEnumerator GameWonCoroutine()
    {
        yield return null;
    }

    public IEnumerator RetryAsyncLoad(string sceneName)
    {
        reloadScene = SceneManager.LoadSceneAsync(sceneName);
        reloadScene.allowSceneActivation = false;
        while (reloadScene.allowSceneActivation == false || reloadScene.progress < 1 || !SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        SaveGameManager.Instance.ReloadDataFromFile();
        SaveGameManager.Instance.LoadAllData();

        if (this != null && this.transform != null && this.transform.parent != null && this.transform.parent.gameObject != null)
        {
            Destroy(this.transform.parent.gameObject);
        }
    }

    public void ShowLoading()
    {
        GameOverOptions.SetActive(false);
        LoadingText.SetActive(true);
    }

    public void LoadTitle()
    {
        StopCoroutine(asyncReloadRoutine);
        ShowLoading();
        GameManager.instance.LoadTitleScreen();
    }

    public void RetryLevel()
    {
        if (reloadScene == null)
        {
            StopCoroutine(asyncReloadRoutine);
            SaveGameManager.Instance.LoadGame();
        }
        else
        {
            ShowLoading();
            reloadScene.allowSceneActivation = true;
        }
    }

}