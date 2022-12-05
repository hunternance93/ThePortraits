using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public GameObject ProceedIcon = null;
    public TextMeshProUGUI TextField = null;
    public Image CurrentPicture = null;
    public GameObject Credits = null;
    public GameObject SkipPopup = null;
    public Image AdditionalPicture = null;

    public SceneList SceneToPlay = SceneList.Intro;

    public Sprite[] NormalEndingSprites;
    public Sprite[] TrueEndingSprites;
    public Sprite[] AlienEndingSprites;

    public AudioSource TrueEndingMusic;
    public AudioSource NormalEndingMusic;
    public AudioSource AlienEndingMusic;
    public AudioSource CreditsMusic;

    private float fadeTime = 1;
    private float fadeTimeImage = 2f;

    public Color32 startColor = new Color32(255, 0, 0, 0);
    public Color32 fadeInColor = new Color32(255, 0, 0, 255);
    public Color32 imageStartColor = new Color32(255, 255, 255, 0);
    public Color32 imageFadeInColor = new Color32(255, 255, 255, 255);
    private CanvasGroup CG;
    public float canvasFadeOutTime = 2.5f;

    public GameObject UFOMiniGame = null;
    public GameObject NonUFOMiniGameUI = null;

    public GameObject BadEndingFlash = null;
    public GameObject GoodEndingFlash = null;

    [HideInInspector] public static SceneList EndingEarned;

    private Coroutine fadingOut = null;
    [HideInInspector] public Coroutine scenePlaying = null;

    private Vector2 currentImageStartPos;

    public enum SceneList
    {
        Intro,
        Ending,
        CreepyPasta
    }


    private void Start()
    {
        CG = GetComponent<CanvasGroup>();

        currentImageStartPos = CurrentPicture.transform.localPosition;
        StartScene();
    }

    public void StartScene()
    {
        switch (SceneToPlay)
        {
            case SceneList.Intro:
                scenePlaying = StartCoroutine(BadEnding());
                break;
            case SceneList.Ending:
                scenePlaying = StartCoroutine(TrueEnding());
                break;
            case SceneList.CreepyPasta:
                scenePlaying = StartCoroutine(CreepyPasta());
                break;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && fadingOut == null)
        {
            SkipPopup.SetActive(!SkipPopup.activeSelf);
        }
    }

    public void SkipEnding()
    {
        if (fadingOut == null)
        {
            StopAllCoroutines();
            fadingOut = StartCoroutine(EndEnding());
        }
    }

    private IEnumerator FadeInText(string text)
    {
        if (TextField.color.a > 0) yield return StartCoroutine(FadeOutText());

        TextField.text = text;

        float timer = 0;
        while (timer < fadeTime)
        {
            TextField.color = Color32.Lerp(startColor, fadeInColor, timer / fadeTime);
            timer += Time.deltaTime;
            yield return null;
        }
        TextField.color = fadeInColor;
    }

    private IEnumerator FadeOutText()
    {
        float timer = 0;
        while (timer < fadeTime)
        {
            TextField.color = Color32.Lerp(fadeInColor, startColor,timer / fadeTime);
            timer += Time.deltaTime;
            yield return null;
        }
        TextField.color = startColor;
        yield return new WaitForSeconds(.2f);
    }

    private IEnumerator FadeInImage(Vector2 newPos, Sprite newImage = null, float width = -1, float height = -1)
    {
        if (CurrentPicture.color.a > 0) yield return StartCoroutine(FadeOutImage());

        if (newPos != null)
        {
            CurrentPicture.transform.localPosition = newPos;
        }

        yield return FadeInImage(newImage, width, height, true);
    }

    private IEnumerator FadeInImage(Sprite newImage = null, float width = -1, float height = -1, bool dontResetPosition = false)
    {
        if (CurrentPicture.color.a > 0) yield return StartCoroutine(FadeOutImage());

        // if (width != -1 && height != -1)
        // {
        //     CurrentPicture.rectTransform.sizeDelta = new Vector2(width, height);
        // }
        //
        // if (!dontResetPosition) CurrentPicture.transform.localPosition = currentImageStartPos;

        if (newImage != null) CurrentPicture.sprite = newImage;

        float timer = 0;
        while (timer < fadeTimeImage)
        {
            CurrentPicture.color = Color32.Lerp(imageStartColor, imageFadeInColor, timer / fadeTimeImage);
            timer += Time.deltaTime;
            yield return null;
        }
        CurrentPicture.color = imageFadeInColor;
    }

    private IEnumerator FadeOutImage()
    {
        float timer = 0;
        while (timer < fadeTimeImage)
        {
            CurrentPicture.color = Color32.Lerp(imageFadeInColor, imageStartColor, timer / fadeTimeImage);
            timer += Time.deltaTime;
            yield return null;
        }
        CurrentPicture.color = imageStartColor;
        yield return new WaitForSeconds(.2f);
    }

    private IEnumerator FadeInAdditionalImage()
    {
        float timer = 0;
        while (timer < fadeTimeImage)
        {
            AdditionalPicture.color = Color32.Lerp(imageStartColor, imageFadeInColor, timer / fadeTimeImage);
            timer += Time.deltaTime;
            yield return null;
        }
        AdditionalPicture.color = imageFadeInColor;
    }

    private IEnumerator FadeInAdditionalImageAfterFadeout()
    {
        yield return StartCoroutine(FadeOutImage());
        StartCoroutine(FadeInAdditionalImage());
    }

    private IEnumerator FadeOutAdditionalImage()
    {
        float timer = 0;
        while (timer < fadeTimeImage)
        {
            AdditionalPicture.color = Color32.Lerp(imageFadeInColor, imageStartColor, timer / fadeTimeImage);
            timer += Time.deltaTime;
            yield return null;
        }
        AdditionalPicture.color = imageStartColor;
    }

    private IEnumerator WaitForProceed()
    {
        bool noInput = true;
        bool ignoreInputNextFrame = false;
        while (noInput)
        {
            if (!ignoreInputNextFrame && !SkipPopup.activeSelf)
            { 
                //The additional checks are for a workaround for weird unity alt tab issue when built out
                if (Keyboard.current.anyKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {   
                    if (!(Keyboard.current.ctrlKey.wasPressedThisFrame || Keyboard.current.altKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.shiftKey.wasPressedThisFrame)) noInput = false;
                }
                else if (Mouse.current.leftButton.wasPressedThisFrame) noInput = false;
            }
            ignoreInputNextFrame = SkipPopup.activeSelf;

            yield return null;
        }
    }

    private IEnumerator FadeOutCanvasGroup()
    {
        float timer = 0;
        while (timer < canvasFadeOutTime)
        {
            if(SceneToPlay == SceneList.Intro)
            {
                //NormalEndingMusic.volume = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            }
            else if (SceneToPlay == SceneList.Ending)
            {
                //AlienEndingMusic.volume = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            }

            CG.alpha = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        CG.alpha = 0;
        if (SceneToPlay == SceneList.Intro)
        {
            //NormalEndingMusic.volume = 0;
            //CreditsMusic.Play();
        }
        else if (SceneToPlay == SceneList.Ending)
        {
            //AlienEndingMusic.volume = 0;
        }
    }

    private IEnumerator FadeInCanvasGroup()
    {
        float timer = 0;
        while (timer < canvasFadeOutTime)
        {
            CG.alpha = Mathf.Lerp(0, 1, timer / canvasFadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        CG.alpha = 1;
    }

    private IEnumerator BadEnding()
    {
        yield return StartCoroutine(FadeInText(@"I was supposed to be home hours ago."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"It was a simple gig, but many hours away, in a small town I had never been."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"A few hours into my ride back my truck started acting up, and I had to stop by the mechanic. I spent an arm and a leg but it seemed to be fixed, and it wasn't till 9PM that they were done with 6 hours left to drive."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I continued the drive home, but all at once it hit me just how much I had been overworked lately. I was exhausted. I nearly swerved into oncoming traffic and realized I couldn't make it."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"There was only one hotel in this town and it was being renovated, so I had to get an AirBNB."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"There was only one available, $130 a night before fees, and with a rating of 1.2. Unbelievable, but can't be worse than some of the places I have stayed."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"It was down a long, winding road in the woods. Eventually, the path narrowed so much that I couldn't take my truck any further."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I followed the path another half mile until finally I see it. More a cabin than a house, really."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I typed in the keycode on the lockbox by the front door. After three tries it opened, and I grabbed the key, went inside the shit-hole, and locked the door behind me."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }

    private IEnumerator CreepyPasta()
    {
        yield return StartCoroutine(FadeInText(@"[This game was inspired by a creepypasta I once read by the name of ""The Portraits"". The author is unknown.]"));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"There was a hunter in the woods, who, after a long day hunting, was in the middle of an immense forest. It was getting dark, and having lost his bearings, he decided to head in one direction until he was clear of the increasingly oppressive foliage."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"After what seemed like hours, he came across a cabin in a small clearing. Realizing how dark it had grown, he decided to see if he could stay there for the night. He approached and found the door ajar."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Nobody was inside."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"The hunter flopped down on the single bed, deciding to explain himself to the owner in the morning. As he looked around, he was surprised to see the walls adorned by many portraits, all painted in incredible detail."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Without exception, they appeared to be staring down at him, their features twisted into looks of hatred. Staring back, he grew increasingly uncomfortable."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@" Making a concerted effort to ignore the many hateful faces, he turned to face the wall, and exhausted, he fell into a restless sleep."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Face down in an unfamiliar bed, he turned blinking in unexpected sunlight."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Looking up, he discovered that the cabin had no portraits, only windows."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }


    private IEnumerator TrueEnding()
    {
        //TrueEndingMusic.Play();

        StartCoroutine(FadeInImage(NormalEndingSprites[1], 1160, 550));
        yield return StartCoroutine(FadeInText(@"Daylight, finally, met me on the other side of the tunnel. I could only have been gone for one long night, but it felt like much longer. Somehow, I found the strength to walk down the tracks. I was physically exhausted, but I felt something had been lifted from me."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I ran into a neighboring town some ways away. Thankfully, the ATM worked so I bought a meal and took a train - a normal one - home."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(NormalEndingSprites[0]));
        yield return StartCoroutine(FadeInText(@"Alarmingly, we passed through Kisaragi, which didn't seem right. But the town was somehow even more run down than I'd just seen, as if it had aged 50 years in the time it took me to leave the train tunnel."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(NormalEndingSprites[2], 700, 905));
        yield return StartCoroutine(FadeInText(@"I got home. My parents and sister were worried. My exams had actually gone alright. The truth is I just had had this overwhelming sense that I couldn't face them."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I had been just so... flooded with this feeling of panic.  My heart was racing at the time and I was so sure it was the curse. Throughout our lives, my mother insisted it was all alright, but I just felt that cloud over me."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Well. Now I understood what had happened to that town; I had as clear a picture as you can get, anyway. I did leave haunted, still - in an inward way."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(NormalEndingSprites[3], 1160, 550));
        yield return StartCoroutine(FadeInText(@"Even having broken the curse, you don't leave an experience like that completely unscarred. But I learned to cope, with time. My therapist and my prescription mostly keep my anxiety in check."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Truthfully, I still get the occasional nightmare of seeing through the eyes of something horrible floating just around the corner..."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(TrueEndingSprites[0], 1160, 550));
        yield return StartCoroutine(FadeInText(@"But, the sense of accomplishment, of doing what Aunt Noriko needed me to do in her stead... it drove me onward."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"The heirloom never glowed again, but I kept it just the same."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }

    private IEnumerator EndEnding()
    {  
        yield return StartCoroutine(FadeOutCanvasGroup());
        StopAllCoroutines();
        scenePlaying = null;
        if (Credits != null) Credits.SetActive(true);
        if (SceneToPlay == SceneList.Intro) SceneManager.LoadScene("Indoors");
        if (SceneToPlay == SceneList.CreepyPasta) SceneManager.LoadScene("TitleScreen");
            gameObject.SetActive(false);
    }
}
