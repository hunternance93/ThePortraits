using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginBossDescentTrigger : MonoBehaviour
{
    public LookAtPlayer Overseer = null;

    public AudioSource[] audioToPlay = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Overseer.BeginCollapse();
            foreach (AudioSource aud in audioToPlay)
            {
                if (!aud.isPlaying) aud.Play();
            }
            //InvisibleWall.SetActive(true);
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
