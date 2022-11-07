using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedInteractable : MonoBehaviour, IInteractable
{
    [HideInInspector] public bool IsPowerOn = false;
    [HideInInspector] public bool UpsetStomach = false;


    public void Interacted()
    {
        if (UpsetStomach)
        {
            GameManager.instance.DisplayMessage("I can't sleep, my stomach is killing me.");
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
        GameManager.instance.DisplayMessage("What an absolute shithole--not even a pillow. And I can't stand these weird portraits but my eyes keep getting drawn to them... I'll just rest my head facedown and hope that I can eventually sleep.", 10);
        yield return new WaitForSeconds(16);
        //TODO: Sound effects...
        //TODO: Make 'portrait' go away
        GameManager.instance.FadeIn();
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
        GameManager.instance.DisplayMessage("My stomach is suddenly killing me...");
    }
}
