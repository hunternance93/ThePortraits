using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedFromThisSideInteractable : MonoBehaviour, IInteractable
{

    [Tooltip("How long success/failure messages should display")]
    [SerializeField] private float messageLength = 1.5f;
    [SerializeField] private AudioSource lockSound = null;
    

    public void Interacted()
    {
        GameManager.instance.DisplayMessage("It is locked from the other side.", messageLength);
        if (lockSound != null && !lockSound.isPlaying) lockSound.Play();
    }
}
