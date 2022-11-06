using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private BedInteractable bed = null;
    [SerializeField] private OpenDoorScript bathroomDoor = null;
    [SerializeField] CinemachineVirtualCamera toiletCam = null;
    [SerializeField] private Transform portrait = null;

    [HideInInspector] public bool HasWindowBeenInspected = false;

    public void Interacted()
    {
        StartCoroutine(TransitionToToiletCam()); //todo delte
        if (bed.UpsetStomach)
        {
            if (bathroomDoor.IsOpen)
            {
                GameManager.instance.DisplayMessage("I can't shit with the door open. I get performance anxiety.");
            }
            else
            {
                //Start shitting sequence
                StartCoroutine(TransitionToToiletCam());
            }
        }
        else
        {
            GameManager.instance.DisplayMessage("I don't need to use the bathroom right now.");
        }
    }

    private IEnumerator TransitionToToiletCam()
    {
        GameManager.instance.FadeOut();
        yield return new WaitForSeconds(1);
        //Set camera
        toiletCam.Priority = 999;
        GameManager.instance.FadeIn();
        while (!HasWindowBeenInspected) yield return null;
        toiletCam.LookAt = portrait;
    }
}
