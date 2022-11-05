using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayedMessage : MonoBehaviour
{
    [SerializeField] private float timeBeforeFadeOut = 7f;
    [SerializeField] private float fadeOutRate = .2f;
    [SerializeField] private TextMeshProUGUI textField = null;
    [SerializeField] private bool isProTipField = false;
    [SerializeField] private bool fadeInMessage = false;
    [SerializeField] private bool disableGameObjectOnCompletion = true;

    private float time = 0;

    private bool fadeInMode = false;

    private void Start()
    {
        if (textField == null)
        {
            if (GetComponent<TextMeshProUGUI>() != null) textField = GetComponent<TextMeshProUGUI>();
        }
    }

    public IEnumerator DisplayNewMessageAfter(string message, float _timeBeforeFadeOut, float timeDelayed)
    {
        yield return new WaitForSeconds(timeDelayed);
        DisplayNewMessage(message, _timeBeforeFadeOut);
    }

    public void DisplayNewMessage(string message, float _timeBeforeFadeOut)
    {
        if (isProTipField)
        {
            if(!GameManager.instance.GetDisplayTipsSetting())
            {
                gameObject.SetActive(false);
                return;
            }
        }

        gameObject.SetActive(true);
        textField.text = message;
        timeBeforeFadeOut = _timeBeforeFadeOut;
        if (!fadeInMessage)
        {
            textField.alpha = 1;
            time = 0;
        }
        else
        {
            textField.alpha = 0;
            fadeInMode = true;
        }
    }

    private void Update()
    {
        if (time >= timeBeforeFadeOut)
        {
            textField.alpha -= fadeOutRate * Time.deltaTime;

            if (textField.alpha <= 0)
            {
                time = 0;
                if(disableGameObjectOnCompletion) gameObject.SetActive(false);
            }
        }
        else
        {
            if (fadeInMode)
            {
                textField.alpha += fadeOutRate * Time.deltaTime;

                if (textField.alpha >= 1)
                {
                    textField.alpha = 1;
                    fadeInMode = false;
                }
            }
            time += Time.deltaTime;
        }
    }
}
