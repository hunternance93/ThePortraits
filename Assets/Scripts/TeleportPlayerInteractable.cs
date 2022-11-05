using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerInteractable : MonoBehaviour, IInteractable
{

    public Transform WhereToTeleport = null;

    private AudioSource aud = null;

    private void Start()
    {
        aud = GetComponent<AudioSource>();
    }

    public void Interacted()
    {
        if (aud != null) aud.Play();
        GameManager.instance.Player.transform.SetPositionAndRotation(WhereToTeleport.position, GameManager.instance.Player.transform.rotation);
    }
}
