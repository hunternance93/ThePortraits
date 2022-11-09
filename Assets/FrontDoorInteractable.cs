using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontDoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject otherDoor = null;

    public void Interacted()
    {
        //TODO: Add sounds
        otherDoor.SetActive(true);
        GameManager.instance.DisplayMessage("So something was in here... Looks like it left.");
        PortraitManager.instance.TerrorGazeEnabled = true;
        gameObject.SetActive(false);
    }
    
}
