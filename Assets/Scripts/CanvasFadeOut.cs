using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFadeOut : MonoBehaviour
{
    [Tooltip("How long the fade should be")]
    public float FadeLength = .5f;
    [SerializeField] private bool fadeOnEnable = false;
    [SerializeField] private bool fadeOnEnableOnlyOnce = false;

    private Image fadeImage = null;

    private bool hasFaded = false;

    private void Awake()
    {
        fadeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (fadeOnEnable && ! hasFaded)
        {
            StartCoroutine(FadeIn());
            if (fadeOnEnableOnlyOnce)
            {
                hasFaded = true;
            }
        }
    }

    //Fade out meaning that the black image appears
    public IEnumerator FadeOut()
    {
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);

        float timer = 0;
        while (timer <= FadeLength)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(0, 1, timer / FadeLength));
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
    }

    //Fade out meaning that the black image appears
    public IEnumerator FadeOut(float length)
    {
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);

        float timer = 0;
        while (timer <= length)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(0, 1, timer / length));
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
    }

    //Fade in meaning that the black image disappears
    public IEnumerator FadeIn()
    {
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);

        float timer = 0;
        while (timer <= FadeLength)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(1, 0, timer / FadeLength));
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
    }

    //Fade in meaning that the black image disappears
    public IEnumerator FadeIn(float length)
    {
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);

        float timer = 0;
        while (timer <= length)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(1, 0, timer / length));
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
    }
}
