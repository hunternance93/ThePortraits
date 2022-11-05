using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flash : MonoBehaviour
{
    public Color32 transparent;// = //new Color32(0, 0, 0, 0);
    public Color32 targetColor;// = //new Color32(255, 255, 255, 255);
    public Color32 partiallyTransparent;
    public AudioSource MagicWhoosh;

    public bool fadeOutOnStart = false;

    private Image img;
    private bool usePartialTransparentImg = false;

    private void Start()
    {
        img = GetComponent<Image>();
        if (fadeOutOnStart) StartCoroutine(FadeOut(5));
    }

    public void SetPartiallyTransparent(bool transparent)
    {
        usePartialTransparentImg = transparent;
    }

    public IEnumerator FadeIn(float length, bool playMagicWhoosh = false)
    {
        if (playMagicWhoosh && MagicWhoosh != null) MagicWhoosh.Play();
        float timer = 0;
        while (timer < length)
        {
            if (!usePartialTransparentImg) img.color = Color32.Lerp(transparent, targetColor, timer / length);
            else img.color = Color32.Lerp(transparent, partiallyTransparent, timer / length);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        if (!usePartialTransparentImg) img.color = targetColor;
        else img.color = partiallyTransparent;
    }

    public IEnumerator FadeOut(float length)
    {
        float timer = 0;
        while (timer < length)
        {
            if (!usePartialTransparentImg) img.color = Color32.Lerp(targetColor, transparent, timer / length);
            else img.color = Color32.Lerp(partiallyTransparent, transparent, timer / length);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        img.color = transparent;
    }

    public IEnumerator FlashScreen(float lengthPer, bool usePartialTransparent = false)
    {
        usePartialTransparentImg = usePartialTransparent;
        if (MagicWhoosh != null) MagicWhoosh.Play();
        yield return StartCoroutine(FadeIn(lengthPer));
        StartCoroutine(FadeOut(lengthPer));
    }
}
