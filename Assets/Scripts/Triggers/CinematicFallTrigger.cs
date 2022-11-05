using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicFallTrigger : MonoBehaviour
{

    [SerializeField] private CanvasFadeOut canvasFadeOut = null;
    [SerializeField] private AudioSource impactGroundNoise = null;
    [SerializeField] private AudioSource hurtKaedeNoise = null;
    [SerializeField] private CinemachineVirtualCamera lookUpHoleCam = null;
    [SerializeField] private GameObject eventEnemy = null;
    [SerializeField] private Transform playerPositionAfterFall = null;

    private float fadeLength = 1;
    private float additionalWaitLengthForFall = .85f;
    private float waitOnFloor = 5;
    private float waitLookingUp = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(CinematicFallRoutine());
        }
    }

    private IEnumerator CinematicFallRoutine()
    {
        GameManager.instance.Player.CanSightJack = false;
        canvasFadeOut.gameObject.SetActive(true);
        StartCoroutine(canvasFadeOut.FadeOut(fadeLength));
        yield return new WaitForSeconds(fadeLength);
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
        yield return new WaitForSeconds(additionalWaitLengthForFall);
        GameManager.instance.Player.transform.position = playerPositionAfterFall.position;
        impactGroundNoise.Play();
        KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.YelpKnockedOut);
        yield return new WaitForSeconds(impactGroundNoise.clip.length);
        hurtKaedeNoise.Play();
        yield return new WaitForSeconds(waitOnFloor);
        lookUpHoleCam.Priority = 51;
        StartCoroutine(canvasFadeOut.FadeIn(fadeLength));
        eventEnemy.SetActive(true);
        yield return new WaitForSeconds(waitLookingUp);
        KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.YelpKnockedOut2);
        eventEnemy.GetComponent<EnemyAI>().TriggerStart();
        yield return new WaitForSeconds(waitLookingUp);
        KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.YelpKnockedOut3);
        StartCoroutine(canvasFadeOut.FadeOut(fadeLength));
        yield return new WaitForSeconds(waitOnFloor);
        lookUpHoleCam.Priority = 0;
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
        StartCoroutine(canvasFadeOut.FadeIn(fadeLength));
        GameManager.instance.Player.CanSightJack = true;
    }
}
