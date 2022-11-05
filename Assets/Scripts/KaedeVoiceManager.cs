using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaedeVoiceManager : MonoBehaviour
{
    public static KaedeVoiceManager instance = null;

    public AudioSource KaedeAudioSource = null;

    public AudioClip ItIsLocked = null;
    public AudioClip MyHead = null;
    public AudioClip HeadacheGroan = null;
    public AudioClip Creepy = null;
    public AudioClip Disgusting = null;
    public AudioClip WhatIsThis = null;
    public AudioClip What = null;
    public AudioClip Panting = null;
    public AudioClip Heavy = null;
    public AudioClip PleaseDontLetItBeHer = null;
    public AudioClip AuntNoriko = null;
    public AudioClip Shit = null;
    public AudioClip OhNo = null;
    public AudioClip Gasp = null;
    public AudioClip HeavyPanting = null;
    public AudioClip SighOfRelief = null;
    public AudioClip GroanHittingFloor = null;
    public AudioClip YelpKnockedOut = null;
    public AudioClip YelpKnockedOut2 = null;
    public AudioClip YelpKnockedOut3 = null;
    public AudioClip Scream = null;
    public AudioClip Scream2 = null;
    public AudioClip Scream3 = null;
    public AudioClip ChasedByMonster1 = null;
    public AudioClip ChasedByMonster2 = null;

    private const float _fadeOutTime = .5f;
    private const int _oddsOfMakingSoundWhenSpotted = 10;
    private const int _oddsOfMakingSoundWhenCaught = 5;

    private bool hasPanted = false;

    private Coroutine fadingOutDialogue = null;

    private void Awake()
    {
        instance = this;
    }

    public void TriggerEnemyAggro(bool shitOnly = false)
    {
        StartCoroutine(TriggerEnemyAudioAfterWait(shitOnly));
    }

    private IEnumerator TriggerEnemyAudioAfterWait(bool shitOnly = false)
    {
        yield return new WaitForSeconds(.3f);
        if (shitOnly)
        {
            MakeKaedeSay(Shit, true);
        }
        else if (Random.Range(1, _oddsOfMakingSoundWhenSpotted) == 1)
        {
            switch (Random.Range(1, 3))
            {
                case 1:
                    MakeKaedeSay(Gasp, true);
                    break;
                case 2:
                    MakeKaedeSay(Shit, true);
                    break;
                case 3:
                    //Should never happen. Leaving here in case we do get sound file for "Oh no!" later
                    MakeKaedeSay(OhNo, true);
                    break;
            }
        }
    }

    public void TriggerScream()
    {
        if (Random.Range(1, _oddsOfMakingSoundWhenCaught) == 1)
        {
            switch (Random.Range(1, 4))
            {
                case 1:
                    MakeKaedeSay(Scream, true);
                    break;
                case 2:
                    MakeKaedeSay(Scream2, true);
                    break;
                case 3:
                    MakeKaedeSay(Scream3, true);
                    break;
            }
        }
    }

    public void TriggerPanting()
    {
        MakeKaedeSay(hasPanted ? HeavyPanting : Panting);
        hasPanted = !hasPanted;
    }

    public void MakeKaedeSay(AudioClip aud, bool overrideCurrentDialogue = false, bool fadeOutOnOverride = false)
    {
        if (fadingOutDialogue != null || aud == null) return;

        if (KaedeAudioSource.isPlaying)
        {
            //If Kaede was panting let's fade that out and play new sound because that might be a somewhat common scenario
            if (!overrideCurrentDialogue && (KaedeAudioSource.clip == Panting || KaedeAudioSource.clip == HeavyPanting))
            {
                overrideCurrentDialogue = true;
                fadeOutOnOverride = true;
            }

                if (overrideCurrentDialogue)
            {
                if (fadeOutOnOverride)
                {
                    fadingOutDialogue = StartCoroutine(FadeOutDialogueAndPlay(aud));
                    return;
                }
                else
                {
                    KaedeAudioSource.Stop();
                }
            }
            else return;
        }

        KaedeAudioSource.clip = aud;
        KaedeAudioSource.Play();
    }

    private IEnumerator FadeOutDialogueAndPlay(AudioClip aud)
    {
        float timer = 0;
        while (timer < _fadeOutTime)
        {
            timer += Time.deltaTime;
            KaedeAudioSource.volume = Mathf.Lerp(1, 0, timer / _fadeOutTime);
            yield return null;
        }
        KaedeAudioSource.Stop();
        KaedeAudioSource.volume = 1;
        KaedeAudioSource.clip = aud;

        KaedeAudioSource.Play();
        fadingOutDialogue = null;
    }
}
