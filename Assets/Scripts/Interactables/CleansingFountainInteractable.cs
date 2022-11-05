using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleansingFountainInteractable : MonoBehaviour, IInteractable
{
    public CoinDepositInteractable[] coinDeposits = null;
    public ShrineDoorInteractable shrineDoor = null;

    public GameObject Water = null;
    public AudioSource PouringWater = null;

    private bool isReadyToCleanse = false;
    private bool AllSetToReady = false;

    public void Interacted()
    {
        if (AllSetToReady)
        {
            GameManager.instance.DisplayMessage("I have already cleansed myself. I even cleaned the handle of the ladle, as if someone else is going to use it after me...");
            return;
        }

        if (isReadyToCleanse)
        {
            if (!shrineDoor.readyToOpen)
            {
                PouringWater.Play();
                GameManager.instance.DisplayMessage("Now that there is water in the basin, I use it to cleanse my hands and mouth.");
                shrineDoor.readyToOpen = true;
            }
            else
            {
                GameManager.instance.DisplayMessage("I have already cleansed myself. I even cleaned the handle of the ladle, as if someone else is going to use it after me...");
            }
        }
        else
        {
            GameManager.instance.DisplayMessage("The basin is completely dry. Guess I can't cleanse myself.");
        }
    }

    public bool CheckCoinDeposits()
    {
        foreach(CoinDepositInteractable coinDeposit in coinDeposits)
        {
            if (!coinDeposit.HasBeenDeposited) return false;
        }
        isReadyToCleanse = true;
        Water.SetActive(true);
        return true;
    }

    public void SetAllToReady()
    {
        AllSetToReady = true;
        foreach (CoinDepositInteractable coinDeposit in coinDeposits)
        {
            coinDeposit.HasBeenDeposited = true;
        }
        Water.SetActive(true);
    }
}
