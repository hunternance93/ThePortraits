using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is intended to randomly play audio for as long as player is in the trigger

public class RNGAudioTrigger : MonoBehaviour
{
    private bool playerInside = false;
    private float timer = 0;
    private float cooldownTimer = 99999;

    public AudioSource[] audioSourcesToPlay = null;
    public float percentChancePerSecond = 10;
    [Tooltip("Set to true if you want this sound to play whether player is in trigger or not")]
    public bool NoTrigger = false;
    [Tooltip("If there are additional sounds you want to check that are not the audio to play.. for example if bell is being rung, don't do ambient bell 'ting'")]
    public AudioSource[] AdditionalSoundsToCheck = null;
    [Tooltip("If there is a cooldown, then it will not play a new sound until that time has passed after ending last sound")]
    public float Cooldown = 0;

    private int lastAudioSourceIndex = -1;

    private void OnTriggerEnter(Collider other)
    {
        if (!NoTrigger && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = true;
            timer = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!NoTrigger && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = false;
        }
    }

    void Update()
    {
        if ((playerInside || NoTrigger) && !IsAudioPlaying())
        {
            if (cooldownTimer < Cooldown)
            {
                cooldownTimer += Time.deltaTime;
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= 1)
                {
                    timer = 0;
                    if (UnityEngine.Random.Range(1, 100) <= percentChancePerSecond)
                    {
                        int rngVal = (int)Mathf.Round(UnityEngine.Random.Range(0, audioSourcesToPlay.Length));
                        if (rngVal == lastAudioSourceIndex) rngVal = (rngVal == audioSourcesToPlay.Length - 1) ? 0 : rngVal + 1;
                        lastAudioSourceIndex = rngVal;
                        audioSourcesToPlay[rngVal].Play();
                        cooldownTimer = 0;
                    }
                }
            }
        }
    }

    private bool IsAudioPlaying()
    {
        foreach(AudioSource aud in audioSourcesToPlay)
        {
            if (aud.isPlaying) return true;
        }
        foreach(AudioSource aud in AdditionalSoundsToCheck)
        {
            if (aud.isPlaying) return true;
        }
        return false;
    }
}
