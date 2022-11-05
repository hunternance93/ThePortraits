using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineDoorInteractable : MonoBehaviour, IInteractable
{
    [HideInInspector]
    public bool readyToOpen = false;

    public Flash FlashObj = null;

    public GameObject[] objectsToDisable = null;

    public AudioSource aud = null;

    public void Interacted()
    {
        if (readyToOpen)
        {
            StartCoroutine(FlashAndOpenDoor());
        }
        else
        {
            GameManager.instance.DisplayMessage("There's a strange force keeping me from opening the door.");
        }
    }

    private IEnumerator FlashAndOpenDoor()
    {
        yield return FlashObj.FlashScreen(.5f);

        foreach(GameObject go in objectsToDisable)
        {
            go.SetActive(false);
        }

        if (aud != null) aud.Play();
    }
}
