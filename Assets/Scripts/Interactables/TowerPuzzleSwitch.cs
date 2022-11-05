using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPuzzleSwitch : MonoBehaviour, IInteractable
{
    [Tooltip("A sound effect that will play when this is interacted with")]
    [SerializeField] private AudioSource soundEffect = null;
    [Tooltip("A cooldown if necessary so you can't spam the sound effect, zero for none")]
    [SerializeField] private float cooldown;
    [Tooltip("Rotates a platter")]
    [SerializeField] private PlatterRotator[] platters;
    [SerializeField] private PourDetector shishi;

    private Animation anim;
    private float nextInteraction;

    private void Start()
    {
        anim = GetComponent<Animation>();

        if (soundEffect == null && GetComponent<AudioSource>() != null) soundEffect = GetComponent<AudioSource>();
        nextInteraction = 0;
    }

    public void Interacted()
    {
        if (Time.time > nextInteraction)
        {
            anim.Play();
            if (soundEffect != null) soundEffect.Play();
            nextInteraction = Time.time + cooldown;

            foreach (PlatterRotator rotator in platters)
            {
                rotator.Activate();
            }
        } 
    }
}