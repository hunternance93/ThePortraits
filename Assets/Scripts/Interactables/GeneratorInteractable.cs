using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorInteractable : MonoBehaviour, IInteractable
{
    private bool hasBeenRun = false;
    [SerializeField] private AudioSource soundEffect = null;
    [SerializeField] private GameObject[] lightsToEnable = null;
    [SerializeField] private BedInteractable bed = null;

    public void Interacted()
    {
        if (!hasBeenRun)
        {

            if (GameManager.instance.Player.InventoryContains("Fuel"))
            {
                hasBeenRun = true;

                if (soundEffect != null) soundEffect.Play();
                GameManager.instance.DisplayMessage("I filled it with fuel.");
                foreach (GameObject go in lightsToEnable) if (go != null) go.SetActive(true);
                bed.IsPowerOn = true;
            }
            else
            {
                GameManager.instance.DisplayMessage("I need to find fuel to fill it with.");
            }
        }
        else
        {
            GameManager.instance.DisplayMessage("It is already back on, but it doesn't seem to do much.");
        }
    }
}
