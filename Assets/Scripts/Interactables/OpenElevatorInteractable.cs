using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenElevatorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] Animator[] anim = null;
    [SerializeField] private float destroyDelay = 3;
    [SerializeField] private AudioSource[] audioToPlay = null;
    [SerializeField] private float audioDelay = 0;

    private bool interacted = false;

    public void Interacted()
    {
        if (!interacted)
        {
            interacted = true;
            foreach (Animator animation in anim) animation.SetBool("IsOpening", true);

            StartCoroutine(PlayAudioAfterDelay());
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private IEnumerator PlayAudioAfterDelay()
    {
        yield return new WaitForSeconds(audioDelay);
        foreach(AudioSource aud in audioToPlay)
        {
            aud.Play();
            yield return new WaitForSeconds(aud.clip.length);
        }
    }
}
