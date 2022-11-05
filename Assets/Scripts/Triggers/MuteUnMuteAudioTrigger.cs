using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteUnMuteAudioTrigger : MonoBehaviour
{
    public bool MuteOnEnter = true;
    public AudioSource aud = null;
    public float volLevel = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (MuteOnEnter)
        {
            aud.volume = 0;
        }
        else
        {
            aud.volume = volLevel;
        }
    }
}
