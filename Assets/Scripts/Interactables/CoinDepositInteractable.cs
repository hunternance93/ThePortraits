using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDepositInteractable : MonoBehaviour, IInteractable
{
    public CleansingFountainInteractable Fountain;
    public string Message = "There is a small hole the size of a coin on the base of the statue with the number 1 next to it. ";
    public string ItemRequired = "Coin #1";
    public AudioSource InsertCoin = null;

    private const float _messageLength = 10;

    [HideInInspector]
    public bool HasBeenDeposited = false;

    public void Interacted()
    {
        if (!HasBeenDeposited)
        {
            if (GameManager.instance.Player.InventoryContains(ItemRequired))
            {
                HasBeenDeposited = true;
                InsertCoin.Play();
                if (Fountain.CheckCoinDeposits())
                {
                    GameManager.instance.DisplayMessage("I can hear the sound of flowing water somewhere after inserting the final coin...", _messageLength);
                }
                else
                {
                    GameManager.instance.DisplayMessage(Message + "I put the corresponding coin inside and the coin slot fades away.", _messageLength);
                }
            }
            else
            {
                GameManager.instance.DisplayMessage(Message, _messageLength);
            }
        }
        else
        {
            GameManager.instance.DisplayMessage("I already put the coin in here.", _messageLength);
        }
    }
}
