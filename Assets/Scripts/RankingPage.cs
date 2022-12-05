using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingPage : MonoBehaviour
{
    public TextMeshProUGUI ModeField;
    public TextMeshProUGUI EndingField;
    public TextMeshProUGUI DeathsField;
    public TextMeshProUGUI TimeField;
    public TextMeshProUGUI JournalEntryField;
    public Image RankingImage;
    public Sprite AlienSprite;
    public Sprite TrueSprite;
    public GameObject ModesUnlockedPopup;

    private CanvasGroup thisCG;

    private Coroutine routine = null;

    private void Start()
    {
        thisCG = GetComponent<CanvasGroup>();
        routine = StartCoroutine(FadeCanvasGroup(true));
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator FadeCanvasGroup(bool fadeIn, bool loadTitle = false)
    {
        float timer = 0;
        while (timer < 1)
        {
            timer += Time.deltaTime;

            if (fadeIn) thisCG.alpha = Mathf.Lerp(0, 1, timer / 1);
            else thisCG.alpha = Mathf.Lerp(1, 0, timer / 1);

            yield return null;
        }
        thisCG.alpha = fadeIn ? 1 : 0;

        if (loadTitle) SceneManager.LoadScene("TitleScreen");

        routine = null;
    }

    void Update()
    {
        if (routine != null)
        {
            return;
        }
        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Keyboard.current.ctrlKey.wasPressedThisFrame || Keyboard.current.altKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.shiftKey.wasPressedThisFrame) return;

            if (!PlayerPrefs.HasKey("GameCompleted") || PlayerPrefs.GetInt("GameCompleted") != 1)
            {
                PlayerPrefs.SetInt("GameCompleted", 1);
                ModesUnlockedPopup.SetActive(true);
            }
            else
            {
                routine = StartCoroutine(FadeCanvasGroup(false, true));
            }
        }
    }
}
