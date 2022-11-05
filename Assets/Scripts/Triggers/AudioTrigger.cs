using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public AudioSource AudioToPlay = null;
    public bool PlayOnlyOnce = false;
    public AudioSource[] AudioToStop = null;
    public float AudioDelay = 0;
    public bool stopAudioInstantly = false;
    public string MakeKaedeSay = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(PlayAudio());
            if (PlayOnlyOnce) GetComponent<BoxCollider>().enabled = false;
            if (!string.IsNullOrEmpty(MakeKaedeSay))
            {
                if (MakeKaedeSay == "Groan") KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.GroanHittingFloor);
                else if (MakeKaedeSay == "SighOfRelief") KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.SighOfRelief);
                else if (MakeKaedeSay == "HeavyPanting") KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.HeavyPanting);
            }
        }
    }

    private IEnumerator PlayAudio()
    {
        yield return new WaitForSeconds(AudioDelay);
        if (AudioToPlay != null && !AudioToPlay.isPlaying) AudioToPlay.Play();
        if (AudioToStop != null)
        {
            foreach (AudioSource a in AudioToStop)
            {
                if (a != null && a.gameObject != null && a.gameObject.activeSelf && a.isPlaying)
                {
                    if (stopAudioInstantly)
                    {
                        if (a != null && a.gameObject != null && a.gameObject.activeSelf) a.Stop();
                    }
                    else
                    {
                        if (a != null && a.gameObject != null && a.gameObject.activeSelf) StartCoroutine(FadeOutAudio(a));
                    }
                }
            }
        }
    }

    private IEnumerator FadeOutAudio(AudioSource a, float timeToFade = 1)
    {
        float t = 0;
        float startVol = a.volume;
        while (t < timeToFade)
        {
            t += Time.deltaTime;
            a.volume = Mathf.Lerp(startVol, 0, t / timeToFade);
            yield return null;
        }
        a.volume = 0;
    }


}
