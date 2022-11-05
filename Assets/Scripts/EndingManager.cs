using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public GameObject ProceedIcon = null;
    public TextMeshProUGUI TextField = null;
    public Image CurrentPicture = null;
    public GameObject Credits = null;
    public GameObject SkipPopup = null;
    public Image AdditionalPicture = null;

    public SceneList SceneToPlay = SceneList.TBD;

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
        NormalEnding,
        TrueEnding,
        AlienEnding,
        AlienEnding2,
        HeirloomSceneInCave,
        HeirloomSceneInShrine,
        TBD
    }


    private void Start()
    {
        CG = GetComponent<CanvasGroup>();

        currentImageStartPos = CurrentPicture.transform.localPosition;

        if (SceneToPlay == SceneList.TBD)
        {
            SceneToPlay = EndingEarned;
        }

        if (BadEndingFlash != null) StartCoroutine(StartSceneAfterFade());
        else StartScene();
    }

    private IEnumerator StartSceneAfterFade()
    {
        if (SceneToPlay == SceneList.NormalEnding)
        {
            BadEndingFlash.SetActive(true);
        }
        else
        {
            GoodEndingFlash.SetActive(true);
        }
        yield return new WaitForSeconds(3.5f);

        StartScene();
    }

    public void StartScene()
    {
        switch (SceneToPlay)
        {
            case SceneList.NormalEnding:
                scenePlaying = StartCoroutine(BadEnding());
                break;
            case SceneList.TrueEnding:
                scenePlaying = StartCoroutine(TrueEnding());
                break;
            case SceneList.AlienEnding:
                scenePlaying = StartCoroutine(AlienEndingPart1());
                break;
            case SceneList.AlienEnding2:
                scenePlaying = StartCoroutine(AlienEndingPart2());
                break;
            case SceneList.HeirloomSceneInCave:
                scenePlaying = StartCoroutine(HeirloomScene());
                break;
            case SceneList.HeirloomSceneInShrine:
                scenePlaying = StartCoroutine(ShrineHeirloomScene());
                break;
            default:
                scenePlaying = StartCoroutine(BadEnding());
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
            if(SceneToPlay == SceneList.NormalEnding)
            {
                NormalEndingMusic.volume = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            }
            else if (SceneToPlay == SceneList.AlienEnding2)
            {
                AlienEndingMusic.volume = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            }

            CG.alpha = Mathf.Lerp(1, 0, timer / canvasFadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        CG.alpha = 0;
        if (SceneToPlay == SceneList.NormalEnding)
        {
            NormalEndingMusic.volume = 0;
            CreditsMusic.Play();
        }
        else if (SceneToPlay == SceneList.AlienEnding2)
        {
            AlienEndingMusic.volume = 0;
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
        NormalEndingMusic.Play();

        /*I can't quite say I "woke up" some time later. It was like I was just there. Outside.
In a version of Kisaragi with thin, hungry-looking people shambling about. In the tunnel 
one moment, outside the next.*/

        StartCoroutine(FadeInImage(NormalEndingSprites[0], 1160, 550));
        yield return StartCoroutine(FadeInText(@"I can't quite say I ""woke up"" some time later. It was like I was just there. Outside. In a version of Kisaragi with thin, hungry - looking people shambling about. In the tunnel one moment, outside the next."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I wandered along the train tracks, shoulders slumped. My phone had no battery,
        and I had nothing to eat or drink for what felt like... a whole day.*/

        yield return StartCoroutine(FadeInText(@"I wandered along the train tracks, shoulders slumped. My phone had no battery, and I had nothing to eat or drink for what felt like... a whole day."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I looked back 
        in the direction of Kisaragi and had a vision - the town once more filled with 
        floating heads, all staring at that giant one above... but this time it was a woman.
        Noriko? Was it her? It didn't seem quite right, but the vision was momentary.*/

        StartCoroutine(FadeInAdditionalImage());
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(FadeInText(@"I looked back in the direction of Kisaragi and had a vision - the town once more filled with floating heads, all staring at that giant one above... but this time it was a woman. Noriko? Was it her? It didn't seem quite right, but the vision was momentary."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I was near collapse when I encountered that neighboring town. An old woman saw me 
        in my haggard state and invited me in for breakfast and a nap on a spare futon. 
        Thankfully, the ATM and phones in town worked, though no luck charging my dead phone.*/

        StartCoroutine(FadeInImage(NormalEndingSprites[1], 1160, 550));
        StartCoroutine(FadeOutAdditionalImage());
        yield return StartCoroutine(FadeInText(@"I was near collapse when I encountered that neighboring town. An old woman saw me in my haggard state and invited me in for breakfast and a nap on a spare futon.  Thankfully, the ATM and phones in town worked, though no luck charging my dead phone."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I got home. My family was worried, but relieved to see me.*/

        yield return StartCoroutine(FadeInText(@"I got home. My family was worried, but relieved to see me."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*My exams had actually gone alright. The truth is, I don't know exactly what I was 
        so afraid of. I just had had this overwhelming feeling of panic. My heart was racing. 
        I was sure it was the curse. I felt I had to go to Kisaragi, finally.*/

        StartCoroutine(FadeInImage(NormalEndingSprites[2], 700, 905));

        yield return StartCoroutine(FadeInText(@"My exams had actually gone alright. The truth is, I don't know exactly what I was so afraid of. I just had had this overwhelming feeling of panic. My heart was racing. I was sure it was the curse. I felt I had to go to Kisaragi, finally."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*And now I had gone. And I had survived. Wasn't that enough?*/

        yield return StartCoroutine(FadeInText(@"And now I had gone. And I had survived. Wasn't that enough?"));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I never had a clear picture of what happened there. I started therapy, I got 
        a prescription. The visions stopped. For a while.*/

        StartCoroutine(FadeInImage(NormalEndingSprites[3], 1160, 550));
        yield return StartCoroutine(FadeInText(@"I never had a clear picture of what happened there. I started therapy, I got a prescription. The visions stopped. For a while."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I kept the heirloom. In Kisaragi, holding it had felt... odd. Some part of it felt comforting,
        but another part of it felt... wrong. Like a warbling in my head when I held it. 
        It got worse as time went by, so I put it in a box and forgot about it.*/

        yield return StartCoroutine(FadeInText(@"I kept the heirloom. In Kisaragi, holding it had felt... odd. Some part of it felt comforting, but another part of it felt... wrong. Like a warbling in my head when I held it. It got worse as time went by, so I put it in a box and forgot about it."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*I learned to cope with the sense of dread. The visions would happen infrequently
        over the years but I still never got a clear picture of the giant face in the sky.
        It looked something like Aunt Noriko but... Colder. Less like 
        the kind aunt I knew and more ... menacing. I dreaded seeing her.*/

        StartCoroutine(FadeInAdditionalImageAfterFadeout());
        yield return StartCoroutine(FadeInText(@"I learned to cope with the sense of dread. The visions would happen infrequently over the years but I still never got a clear picture of the giant face in the sky. It looked something like Aunt Noriko but... Colder. Less like the kind aunt I knew and more ... menacing. I dreaded seeing her."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*And yet, the visions returned despite changes in my life. Even as the years went by, 
        even when my sister got killed in that accident, even when I got married and 
        even when my daughter was born.*/

        yield return StartCoroutine(FadeInText(@"And yet, the visions returned despite changes in my life. Even as the years went by, even when my sister got killed in that accident, even when I got married and even when my daughter was born."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /* I loved my baby more than anything, little Tomoko. 
        And I felt that woman's face... her eyes seemed to focus more intently when I held her.*/

        yield return StartCoroutine(FadeInText(@"I loved my baby more than anything, little Tomoko. And I felt that woman's face... her eyes seemed to focus more intently when I held her."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*One day, the panic came back, impossible to ignore. So I said silent goodbyes and 
        found that cargo train back to Kisaragi. I can't say it was to finish what I'd 
        started - it was too late for that.*/

        yield return StartCoroutine(FadeInText(@"One day, the panic came back, impossible to ignore. So I said silent goodbyes and found that cargo train back to Kisaragi. I can't say it was to finish what I'd started - it was too late for that."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        /*It felt like Noriko needed me to find something else she'd left behind.*/

        yield return StartCoroutine(FadeInText(@"It felt like Noriko needed me to find something else she'd left behind."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }

    private IEnumerator AlienEndingPart1()
    {
        AlienEndingMusic.Play();

        StartCoroutine(FadeInImage(NormalEndingSprites[1], 1160, 550));
        yield return StartCoroutine(FadeInText(@"Kaede ran, full tilt, out of that godforsaken town. If there was one thing she was done with, it was small town horror. Eventually she reached the next town over, palm-slapped her card into an ATM and went to a bar. ""I need a beer."" The barkeep probably thought it was early but one look at her face and he knew not to refuse."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(AlienEndingSprites[0], 1160, 550));
        yield return StartCoroutine(FadeInText(@"She stepped out, ready to go home when she felt herself lift off the ground. ""Oh, what now?"" She was being abducted!"));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        StartCoroutine(FadeInImage(AlienEndingSprites[1], 1160, 550));
        yield return StartCoroutine(FadeInText(@"The aliens had large heads, but... I mean. Not that big, all things considered."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""We need your help, Kaede,"" the h- er, leader alien said. ""My name is Data Sushi. Our planet has been overrun with monsters that have big heads. They're huge!"""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""How are you speaking Japanese?"" Kaede asked."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""Google Translate. Look, we have a lot of advanced technology like that, but we really need someone who can get in their... h - heads. Sorry for the pun. It is what it is."""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Kaede nodded. ""Well I can certainly get in their heads. I am very empathetic."""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Data Sushi pulled his alien collar. ""No no, I mean the thing where you can see through their eyes. I don't really give a shit what they're thinking or feeling. They're giant heads. Oh no! Wait, some are coming right now! Quick! Grab the wheel of our UFO and shoot em up!"""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Control the ship with A and D. Use Spacebar to fire the laser. Don't let any get by!"));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }

    private IEnumerator AlienEndingPart2()
    {
        CurrentPicture.sprite = AlienEndingSprites[1];
        //CurrentPicture.rectTransform.sizeDelta = new Vector2(1150, 550);

        StartCoroutine(FadeInCanvasGroup());

        yield return StartCoroutine(FadeInText(@"""I'm glad we destroyed them,"" Data Sushi said."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""Well now what?"" Kaede asked."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""I'm thinking we either put you in a pocket dimension where you fight evil for all eternity, or we erase your memory and you get to go home and we all just do the good ending and pretend this part never happened."""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"""Well,"" mused Kaede. ""Let's just go with the good ending."""));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        SkipEnding();
        scenePlaying = null;
    }

    private IEnumerator TrueEnding()
    {
        TrueEndingMusic.Play();

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

    private IEnumerator HeirloomScene()
    {
        yield return StartCoroutine(FadeInText(@"Kaede..."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I knew you would find your way here and fulfill our family destiny. I know it is too late for me, and unfortunately the heirloom is powerless here."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"All you have to do now is get out of this horrible town. Once you are out of its influence, the heirloom will protect you. I know you can do this."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Farewell, my niece."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(EndEnding());
        scenePlaying = null;
    }

    private IEnumerator ShrineHeirloomScene()
    {
        //StartCoroutine(FadeInImage(NorikoPicFromIntro));
        yield return StartCoroutine(FadeInText(@"I am so sorry you came here, Kaede."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I never would have wanted you to come to this dreadful place or put this responsibility on you. I know I don't have long left, but I am cursed by visions of you coming to this place and the only solace I can find is the vision of you finding me here."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"I was captured and locked here by a former friend. I often confided in her--and I often talked about you, and for that I am truly sorry."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"She changed when her husband died. Most likely, Meio was responsible, at the least she believed it was him. She became an outcast but I never expected her to be capable of something like this."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"She began impersonating me in an attempt to bring you here. I do not fully understand her plans, but she created an artifact based on our family heirloom intended to somehow usurp Meio's ascension. She needed you to obtain it and use its power."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"However, with this--our true family heirloom, I know that you can escape and free yourself from this curse. Please leave, and never return. Live a great life and put this place behind you."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(FadeInText(@"Goodbye, little Kaede."));
        ProceedIcon.SetActive(true);
        yield return StartCoroutine(WaitForProceed());
        ProceedIcon.SetActive(false);

        yield return StartCoroutine(EndEnding());
        scenePlaying = null;
    }

    private IEnumerator EndEnding()
    {
        if (SceneToPlay == SceneList.AlienEnding)
        {
            StartCoroutine(FadeOutText());
            yield return StartCoroutine(FadeOutCanvasGroup());
            StopAllCoroutines();
            SceneToPlay = SceneList.AlienEnding2;
            fadingOut = null;
            //Play Schmup
            UFOMiniGame.SetActive(true);
            NonUFOMiniGameUI.SetActive(false);
        }
        else if (SceneToPlay == SceneList.AlienEnding2)
        {
            StartCoroutine(FadeOutText());
            StartCoroutine(FadeOutImage());
            yield return StartCoroutine(FadeOutCanvasGroup());
            StopAllCoroutines();
            SceneToPlay = SceneList.TrueEnding;
            StartScene();
            StartCoroutine(FadeInCanvasGroup());
            fadingOut = null;
        }
        else {
            yield return StartCoroutine(FadeOutCanvasGroup());
            StopAllCoroutines();
            scenePlaying = null;
            if (Credits != null) Credits.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
