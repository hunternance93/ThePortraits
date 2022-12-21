using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    public float LengthOfCredits = 120;

    public GameObject SkipPopup = null;

    public AudioSource audToFade = null;

    private float timer = 0;
    public float targetY = 0;
    private Vector3 startPos;
    private Vector3 tarPos;

    private Coroutine fadeOutRoutine = null;
    private CanvasGroup CG;
    private TMPro.TextMeshProUGUI TMP;
    private float canvasFadeOutTime = 1;

    private bool initialized = false;

    private void Awake()
    {
        TextAsset credits = Resources.Load<TextAsset>("credits");
        TMP = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        TMP.text = credits.text;
        Time.timeScale = 1;
    }

    private void OnEnable()
    {
        CG = GetComponentInParent<CanvasGroup>();
    }

    private void Initialize()
    {
        if (targetY == 0)
        {
            float canvasHeight = GetComponentInParent<RectTransform>().rect.height;
            float textHeight = TMP.bounds.size.y;
            startPos = transform.localPosition;
            targetY = textHeight - (canvasHeight * 5) - transform.localPosition.y;
        }
        tarPos = new Vector3(transform.localPosition.x, targetY, transform.localPosition.z);
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            Initialize();
        }
        
        timer += Time.deltaTime;

        transform.localPosition = Vector3.Lerp(startPos, tarPos, timer / LengthOfCredits);

        if (Keyboard.current.escapeKey.wasPressedThisFrame && fadeOutRoutine == null)
        {
            SkipPopup.SetActive(!SkipPopup.activeSelf);
        }

        if (timer >= LengthOfCredits && fadeOutRoutine == null)
        {
            fadeOutRoutine = StartCoroutine(FadeOutCanvasGroupCreditsEnd());
        }
    }

    public void SkipCredits()
    {
        if (fadeOutRoutine == null) fadeOutRoutine = StartCoroutine(FadeOutCanvasGroup());
    }

    private IEnumerator FadeOutCanvasGroupCreditsEnd()
    {
        yield return new WaitForSeconds(4);
        
        float fadeTimer = 0;
        while (fadeTimer < canvasFadeOutTime)
        {
            audToFade.volume = Mathf.Lerp(1, 0, fadeTimer / canvasFadeOutTime);
            CG.alpha = Mathf.Lerp(1, 0, fadeTimer / canvasFadeOutTime);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        audToFade.volume = 0;
        CG.alpha = 0;
        PlayerPrefs.SetInt("HasBeatenGame", 1);
        SceneManager.LoadScene("TitleScreen");
    }
    
    private IEnumerator FadeOutCanvasGroup()
    {
        if (timer < LengthOfCredits)
        {
            float fadeTimer = 0;
            while (fadeTimer < canvasFadeOutTime)
            {
                audToFade.volume = Mathf.Lerp(1, 0, fadeTimer / canvasFadeOutTime);
                CG.alpha = Mathf.Lerp(1, 0, fadeTimer / canvasFadeOutTime);
                fadeTimer += Time.deltaTime;
                yield return null;
            }
            audToFade.volume = 0;
            CG.alpha = 0;
            PlayerPrefs.SetInt("HasBeatenGame", 1);

            SceneManager.LoadScene("TitleScreen");
        }
    }
}
