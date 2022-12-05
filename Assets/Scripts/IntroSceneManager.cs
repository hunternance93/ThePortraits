using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IntroSceneManager : MonoBehaviour
{
    public GameObject ReceivedMessagePrefab;
    public GameObject SentMessagePrefab;
    public GameObject TimeStampPrefab;
    public GameObject ReceivedMessageParent;
    public GameObject SentMessageParent;
    public GameObject ScrollObject;
    public TextMeshProUGUI TypeMessageField;
    public GameObject ProceedIcon = null;
    public Image CurrentPicture = null;
    public GameObject SkipPopup = null;
    public Image FadeOut = null;
    public TitleScreenStartGameController TitleController;
    public Color32 imageStartColor = new Color32(255, 255, 255, 0);
    public Color32 imageFadeInColor = new Color32(255, 255, 255, 255);
    public AudioSource TrainAudio;
    public AudioSource PhoneBeep;
    public GameObject BlinkImage = null;

    public Sprite[] images;

    private float fadeTimeImage = 2f;

    private float lastMessageYPosition;
    private float offsetPerMessage = 5;
    private float offsetPerTimestamp = 5;
    private float speedOfScroll = .5f;
    private float typeSpeed = .05f;

    private Vector2 currentImageStartPos;


    private void Start()
    {
        lastMessageYPosition = ReceivedMessageParent.transform.localPosition.y - 100;
        currentImageStartPos = CurrentPicture.transform.localPosition;
        StartCoroutine(IntroCutscene());
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkipPopup.SetActive(!SkipPopup.activeSelf);
        }
    }
    
    private IEnumerator Fade()
    {
        Color32 blackTransparent = new Color32(0, 0, 0, 0);
        Color32 black = new Color32(0, 0, 0, 255);

        float timer = 0;
        while (timer < fadeTimeImage)
        {
            FadeOut.color = Color32.Lerp(blackTransparent, black, timer / fadeTimeImage);
            timer += Time.deltaTime;
            yield return null;
        }
        FadeOut.color = black;
        yield return new WaitForSeconds(1);
        timer = 0;
        while (timer < (fadeTimeImage + 1))
        {
            FadeOut.color = Color32.Lerp(black, blackTransparent, timer / (fadeTimeImage + 1));
            timer += Time.deltaTime;
            yield return null;
        }
        FadeOut.color = blackTransparent;
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

        if (width != -1 && height != -1)
        {
            CurrentPicture.rectTransform.sizeDelta = new Vector2(width, height);
        }

        if (!dontResetPosition) CurrentPicture.transform.localPosition = currentImageStartPos;

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

    private IEnumerator WaitForProceed()
    {
        ProceedIcon.SetActive(true);
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
        ProceedIcon.SetActive(false);
    }

    private IEnumerator ReceiveMessage(string message, string timeStamp = null)
    {
        PhoneBeep.Play();

        float scrollDistance = 0;

        if (!string.IsNullOrEmpty(timeStamp))
        {
            GameObject timeStampObj = Instantiate(TimeStampPrefab);
            timeStampObj.GetComponent<TextMeshProUGUI>().text = timeStamp;
            timeStampObj.transform.SetParent(ReceivedMessageParent.transform, false);
            scrollDistance = timeStampObj.GetComponent<RectTransform>().rect.height + offsetPerTimestamp;
            lastMessageYPosition -= scrollDistance / 2;
            timeStampObj.transform.localPosition = new Vector3(100, lastMessageYPosition, 0);
            lastMessageYPosition -= scrollDistance/2;
        }

        GameObject received = Instantiate(ReceivedMessagePrefab);
        TextMeshProUGUI text = received.GetComponentInChildren<TextMeshProUGUI>();
        text.text = message;
        received.transform.SetParent(ReceivedMessageParent.transform, false);
        Canvas.ForceUpdateCanvases();
        float offset = received.GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>().rect.height + offsetPerMessage;
        lastMessageYPosition -= offset/2;
        received.transform.localPosition = new Vector3(-50, lastMessageYPosition, 0);
        lastMessageYPosition -= offset / 2;
        scrollDistance += offset;
        yield return ScrollUpView(scrollDistance);
    }

    private IEnumerator SendTextMessage(string message)
    {
        InstantDeleteMessage();

        float scrollDistance = 0;

        GameObject sent = Instantiate(SentMessagePrefab);
        TextMeshProUGUI text = sent.GetComponentInChildren<TextMeshProUGUI>();
        text.text = message;
        sent.transform.SetParent(SentMessageParent.transform, false);
        Canvas.ForceUpdateCanvases();
        float offset = sent.GetComponentInChildren<Image>().gameObject.GetComponent<RectTransform>().rect.height + offsetPerMessage;
        lastMessageYPosition -= offset / 2;
        sent.transform.localPosition = new Vector3(50, lastMessageYPosition, 0);
        lastMessageYPosition -= offset / 2;
        scrollDistance += offset;
        yield return ScrollUpView(scrollDistance);
    }

    private IEnumerator WriteMessage(string message, bool dontTurnBlinkBackOn = false)
    {
        InstantDeleteMessage();
        BlinkImage.SetActive(false);
        foreach (char c in message)
        {
            TypeMessageField.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        if (!dontTurnBlinkBackOn) BlinkImage.SetActive(true);
    }

    private IEnumerator DeleteMessage()
    {
        for(int i = TypeMessageField.text.Length - 1; i >= 0; i--)
        {
            TypeMessageField.text = TypeMessageField.text.Substring(0, i);
            yield return new WaitForSeconds(typeSpeed/2);
        }
    }

    private void InstantDeleteMessage()
    {
        TypeMessageField.text = "";
    }

    private IEnumerator ScrollUpView(float distanceToTravel)
    {
        Vector3 scrollStartPos = ScrollObject.transform.localPosition;
        Vector3 scrollTargetPos = new Vector3(ScrollObject.transform.localPosition.x, ScrollObject.transform.localPosition.y + distanceToTravel, ScrollObject.transform.localPosition.z);
        float timer = 0;
        while (timer < speedOfScroll)
        {
            timer += Time.deltaTime;
            ScrollObject.transform.localPosition = Vector3.Lerp(scrollStartPos, scrollTargetPos, timer / speedOfScroll);
            yield return null;
        }
        ScrollObject.transform.localPosition = scrollTargetPos;
    }

    private IEnumerator IntroCutscene()
    {
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(ReceiveMessage("kaede are you ok", "21:30"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("kaede you're scaring mom and i, where are you", "09:36"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("I'm Fine Tomoko"));
        yield return StartCoroutine(SendTextMessage("I'm Fine Tomoko"));
        yield return WaitForProceed();

        //Switch Image to Kaede having trouble taking exam
        StartCoroutine(FadeInImage(new Vector2(.379f, 80), images[2], 560, 724));

        yield return StartCoroutine(ReceiveMessage("we thought you'd come home after finals"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("where have you been"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("I had a panic at", true));
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("I wasn't feeling well", true));
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("I just wanted to have some time alone"));
        yield return StartCoroutine(SendTextMessage("I just wanted to have some time alone"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("we called the university"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("your roommate said you were talking about going to aunt noriko's village"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("you didn't really go there did you"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("i'll be fine tomoko, i'll be home tomorrow"));
        yield return StartCoroutine(SendTextMessage("i'll be fine tomoko, i'll be home tomorrow"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("it's just that"));
        yield return StartCoroutine(SendTextMessage("it's just that"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("finals didn't go well", true));
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("i've been having those hallucinations again", true));
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(DeleteMessage());

        //Change image to Noriko
        StartCoroutine(FadeInImage(images[1], 800, 800));

        yield return StartCoroutine(WriteMessage("I just felt bad. The last letter Noriko sent me was very weird and urgent and has me worried."));
        yield return StartCoroutine(SendTextMessage("I just felt bad. The last letter Noriko sent me was very weird and urgent and has me worried."));
        yield return StartCoroutine(WriteMessage("and she always writes us and we never even see her. I thought I'd go there and surprise her."));
        yield return StartCoroutine(SendTextMessage("and she always writes us and we never even see her. I thought I'd go there and surprise her."));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("seriously?! there's not even regular trains that go there. there like weird ones that have cargo and you have to basically hitch a ride"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("that's why mom never goes"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("that's not why", true));
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("i know mom thinks noriko is crazy but... when I was little, when you were a baby"));
        yield return StartCoroutine(SendTextMessage("i know mom thinks noriko is crazy but... when I was little, when you were a baby"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("noriko was nice to me and talked to me about"));
        yield return StartCoroutine(SendTextMessage("noriko was nice to me and talked to me about"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("things i can see", true));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("i guess i panicked", true));
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("it just felt like ... she always talked about the family curse and"));
        yield return StartCoroutine(SendTextMessage("it just felt like ... she always talked about the family curse and"));
        yield return WaitForProceed();
        yield return StartCoroutine(WriteMessage("it's stupid but i wanted to talk to her. just see her"));
        yield return StartCoroutine(SendTextMessage("it's stupid but i wanted to talk to her. just see her"));
        yield return WaitForProceed();
        yield return new WaitForSeconds(3);
        yield return StartCoroutine(ReceiveMessage("you don't really believe in family curses do you?"));
        yield return WaitForProceed();
        yield return new WaitForSeconds(3);
        yield return StartCoroutine(WriteMessage("i don't know anymore", true));
        TrainAudio.Play();
        yield return StartCoroutine(DeleteMessage());
        yield return StartCoroutine(WriteMessage("i have to go the train is about to depart", true));
        yield return StartCoroutine(DeleteMessage());
        BlinkImage.SetActive(true);
        yield return StartCoroutine(ReceiveMessage("is everything ok?"));
        yield return WaitForProceed();
        TrainAudio.Stop();
        yield return StartCoroutine(WriteMessage("and i know the heirloom noriko mentioned probably isn't anything", true));
        yield return StartCoroutine(DeleteMessage());
        BlinkImage.SetActive(true);
        yield return StartCoroutine(ReceiveMessage("is there even cell phone service there?"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("will you even be able to get back?"));
        yield return WaitForProceed();

        yield return new WaitForSeconds(3);

        yield return StartCoroutine(ReceiveMessage("kaede are you there", "11:49"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("are you on the train"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("please answer"));
        yield return WaitForProceed();

        yield return new WaitForSeconds(3);

        StartCoroutine(FadeInImage(images[0], 600, 600));


        yield return StartCoroutine(ReceiveMessage("Kaede mom called the police, we don't know where you are", "17:23"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("isn't your graduation ceremony next week?"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("you didn't really go to the village right?"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("did something happen at school?"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("mom told me to tell you to forget about noriko and whatever she told you about the visions"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("mom told me to tell you please forget the heirloom and come home"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("kaede please"));
        yield return WaitForProceed();

        yield return new WaitForSeconds(3);

        yield return StartCoroutine(ReceiveMessage("please answer", "19:40"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("please answer"));
        yield return WaitForProceed();
        yield return StartCoroutine(ReceiveMessage("kaede?"));
        yield return WaitForProceed();
    }
}
