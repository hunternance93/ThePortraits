using UnityEngine;

public class UnlockableDoorInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Whether or not player needs an item in inventory to destroy this object")]
    [SerializeField] private bool requiresItem = false;
    [Tooltip("Name of item(key) required for player to open this door")]
    [SerializeField] private string item = "";
    [Tooltip("Message displayed if player does not have the key")]
    [SerializeField] private string failureMessage = "I don't have the key to open this door.";
    [Tooltip("Message displayed when player opens the door")]
    [SerializeField] private string successMessage = "You unlock the door.";
    [Tooltip("Array of the OpenDoorScripts of the doors to open")]
    [SerializeField] private OpenDoorScript[] doorsToOpen = null;
    [Tooltip("If this is true, the door will open in the opposite direction as normal")]
    [SerializeField] private bool openDoorOtherDirection = false;
    [Tooltip("Array of objects that will be destroyed when door is unlocked (for example interactables). Do not include the object this is attached to, it will be destroyed at end regardless.")]
    [SerializeField] private GameObject[] objectsToDestroy = null;
    [Tooltip("Array of objects that should be enabled when door is unlocked")]
    [SerializeField] private GameObject[] objectsToEnable = null;
    [Tooltip("How long success/failure messages should display")]
    [SerializeField] private float messageLength = 1.5f;
    [Tooltip("Sound effect that will play when the door is attempted to be opened but is locked")]
    [SerializeField] private AudioSource lockedNoise = null;
    [Tooltip("Sound effect that will play when the door is opened")]
    [SerializeField] private AudioSource openNoise = null;
    [SerializeField] private bool dontAllowUnlockIfAggro = false;
    [SerializeField] private bool playKaedeVoiceLineOnce = false;
    [Tooltip("Used to determine if special message should appear if you have Servants' Quarters Key")]
    [SerializeField] private bool towerDoor = false;

    private bool hasSaidLine = false;

    public void Interacted()
    {
        if (dontAllowUnlockIfAggro)
        {
            if (GameManager.instance.Player.EnemyAggroCount > 0)
            {
                GameManager.instance.DisplayMessage("I can't open it while I'm being chased!");
                return;
            }
        }

        if (requiresItem && item != "")
        {
            if (GameManager.instance.Player.InventoryContains(item))
            {
                foreach (OpenDoorScript ods in doorsToOpen)
                {
                    if (openDoorOtherDirection) ods.ReverseOpenDirection();
                    ods.OpenDoor();
                }
                foreach(GameObject go in objectsToDestroy)
                {
                    Destroy(go);
                }
                foreach (GameObject go in objectsToEnable)
                {
                    go.SetActive(true);
                }
                if (openNoise != null) openNoise.Play();
                if (!string.IsNullOrEmpty(successMessage)) GameManager.instance.DisplayMessage(successMessage, messageLength);
                Destroy(gameObject);
            }
            else
            {
                
                if (lockedNoise != null && !lockedNoise.isPlaying) lockedNoise.Play();
                if (towerDoor && GameManager.instance.Player.InventoryContains("Servants' Quarters Key"))
                {
                    GameManager.instance.DisplayMessage("This key doesn't fit the door.", messageLength);
                }
                else
                {
                    GameManager.instance.DisplayMessage(failureMessage, messageLength);
                    if (playKaedeVoiceLineOnce && !hasSaidLine)
                    {
                        hasSaidLine = true;
                        KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.ItIsLocked);
                    }
                }
            }
        }
        else
        {
            foreach (OpenDoorScript ods in doorsToOpen)
            {
                if (openDoorOtherDirection) ods.ReverseOpenDirection();
                ods.OpenDoor();
            }
            foreach (GameObject go in objectsToDestroy)
            {
                Destroy(go);
            }
            foreach (GameObject go in objectsToEnable)
            {
                go.SetActive(true);
            }
            if (openNoise != null) openNoise.Play();
            GameManager.instance.DisplayMessage(successMessage, messageLength);
            Destroy(gameObject);
        }
    }

    //For when door should start open at beginning of scene after loading a checkpoint
    public void OpenDoorInstantly()
    {
        foreach (OpenDoorScript ods in doorsToOpen)
        {
            ods.OpenDoorInstant();
        }
        foreach (GameObject go in objectsToDestroy)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        foreach (GameObject go in objectsToEnable)
        {
            go.SetActive(true);
        }

        /* "this" has some calls overrided by Unity, and will present null if the object has already had Destroy called, but is not fully Destroyed yet.
           Usually will happen if an OpenDoorScript has this object as an objectToDelete, and this door is opening as part of a save load.
           So we're left with the odd side effect that we get a nullReferenceException when we try this.gameObject, so we need to check "this" first.*/
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
