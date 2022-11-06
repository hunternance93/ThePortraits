using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private OpenDoorScript ods = null;
    public void Interacted()
    {
        ods.OpenDoor();
    }
}
