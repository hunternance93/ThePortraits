using UnityEngine;
using UnityEngine.SceneManagement;

public class InformationInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Message displayed when interacted")]
    [SerializeField] private string message = "";
    [Tooltip("(Optional) How long message should display for")]
    [SerializeField] private float messageLength = -1;
    [Tooltip("(Optional) Should a sound play when you inspect this?")]
    [SerializeField] private AudioSource aud = null;
    [Tooltip("(Optional) What should Kaede say?")]
    [SerializeField] private string kaedeDialogue = "";
    [SerializeField] private ToiletInteractable toilet = null;

    [SerializeField] private bool isFrontDoor = false;

    private bool hasKaedeSpoken = false;

    public void Interacted()
    {
        if (isFrontDoor)
        {
            if (PortraitManager.instance.GetPhase() == 3)
            {
                SceneManager.LoadScene("Ending");
            }
        }
        if (messageLength < 0) GameManager.instance.DisplayMessage(message);
        else GameManager.instance.DisplayMessage(message, messageLength);
        if (aud != null && !aud.isPlaying) aud.Play();
        if (!hasKaedeSpoken && !string.IsNullOrEmpty(kaedeDialogue))
        {
            hasKaedeSpoken = true;
            switch (kaedeDialogue)
            {
                case "Creepy":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.Creepy);
                    break;
                case "Disgusting":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.Disgusting);
                    break;
                case "AuntNoriko":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.AuntNoriko);
                    break;
                case "PleaseDontLetItBeHer":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.PleaseDontLetItBeHer);
                    break;
            }
        }
        if (toilet != null)
        {
            toilet.HasWindowBeenInspected = true;
            Destroy(gameObject);
        }
    }

    public void ChangeMessage(string messageReplacement)
    {
        message = messageReplacement;
    }
}
