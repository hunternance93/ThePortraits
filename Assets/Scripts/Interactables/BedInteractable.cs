using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [HideInInspector] public bool IsPowerOn = false;
    [HideInInspector] public bool UpsetStomach = false;

    [SerializeField] private ToiletInteractable toilet = null;
    [SerializeField] private GameObject openedDoor = null;
    [SerializeField] private OpenDoorScript bedroomDoor = null;


    public void Interacted()
    {
        if (UpsetStomach)
        {
            if (!toilet.HasTakenShit)
            {
                GameManager.instance.DisplayMessage("I can't sleep, my stomach is killing me.");
            }
            else
            {
                if (openedDoor.activeSelf)
                {
                    GameManager.instance.DisplayMessage("I can't go back to bed until I make sure it is safe.");
                }
                else if (bedroomDoor.IsOpen)
                {
                    GameManager.instance.DisplayMessage("I should close the door to be safe.");
                }
                else
                {
                    //TODO: New sleep event
                    StartCoroutine(SleepRoutine2());
                }
            }
        }
        else
        {
            if (!IsPowerOn)
            {
                GameManager.instance.DisplayMessage("I'm exhausted, but I should probably try to get the power on first still.");
            }
            else
            {
                StartCoroutine(SleepRoutine());
            }
        }
    }

    private IEnumerator SleepRoutine()
    {
        UpsetStomach = true;
        GameManager.instance.FadeOut();
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        GameManager.instance.DisplayMessage("How the hell am I supposed to sleep with these staring at me? I'm trying to ignore them but it is getting in my head. I'll just rest my head facedown and hope that I can eventually sleep.", 10);
        yield return new WaitForSeconds(16);
        //TODO: Sound effects... Scratching?
        //TODO: Make 'portrait' go away
        GameManager.instance.FadeIn();
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
        GameManager.instance.DisplayMessage("My stomach is suddenly killing me...");
    }

    private IEnumerator SleepRoutine2()
    {
        GameManager.instance.FadeOut();
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        GameManager.instance.DisplayMessage("They're still looking at me, but it isn't safe to go outside. I will have to wait till morning...", 10);
        yield return new WaitForSeconds(16);
    }
}