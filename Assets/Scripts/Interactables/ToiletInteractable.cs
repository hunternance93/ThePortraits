using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private BedInteractable bed = null;
    [SerializeField] private OpenDoorScript bathroomDoor = null;
    [SerializeField] CinemachineVirtualCamera toiletCam = null;
    [SerializeField] private Transform portrait = null;
    [SerializeField] private GameObject inspectWindow = null;
    [SerializeField] private bool testMode = false;
    [SerializeField] private AudioSource risingTensionAudio = null;
    [SerializeField] private AudioSource running = null;
    [SerializeField] private AudioSource doorOpen = null;
    [SerializeField] private AudioSource huh = null;
    [SerializeField] private InformationInteractable basementDoor = null;
    [SerializeField] private GameObject onFrontDoor = null;
    [SerializeField] private GameObject offFrontDoor = null;

    [HideInInspector] public bool HasWindowBeenInspected = false;
    [HideInInspector] public bool HasTakenShit = false;
    private Coroutine ShitRoutine = null;

    private float startRecenter = 10;
    private float finishRecenter = .01f;
    private float startWait = 3;
    private float finishWait = .01f;

    public void Interacted()
    {
        if (ShitRoutine != null)
        {
            return;
        }
        if (HasTakenShit)
        {
            GameManager.instance.DisplayMessage("Who is here with me?");
        }
        else
        {
            if (testMode) StartCoroutine(TransitionToToiletCam());
            if (bed.UpsetStomach)
            {
                if (bathroomDoor.IsOpen)
                {
                    GameManager.instance.DisplayMessage("I can't shit with the door open. I get performance anxiety.");
                }
                else
                {
                    //Start shitting sequence
                    StartCoroutine(TransitionToToiletCam());
                }
            }
            else
            {
                GameManager.instance.DisplayMessage("I don't need to use the bathroom right now.");
            }
        }
    }

    private IEnumerator TransitionToToiletCam()
    {
        GameManager.instance.FadeOut();
        GameManager.instance.IsShitting = true;
        yield return new WaitForSeconds(1);
        //Set camera
        toiletCam.Priority = 999;
        CinemachinePOV POV = toiletCam.GetCinemachineComponent<CinemachinePOV>();
        POV.m_HorizontalRecentering.m_RecenteringTime = .01f;
        POV.m_HorizontalRecentering.m_WaitTime = .01f;
        POV.m_VerticalRecentering.m_RecenteringTime = .01f;
        POV.m_VerticalRecentering.m_WaitTime = .01f;
        yield return new WaitForSeconds(.1f);
        POV.m_HorizontalRecentering.m_enabled = false;
        POV.m_VerticalRecentering.m_enabled = false;
        POV.m_HorizontalRecentering.m_RecenteringTime = 10;
        POV.m_HorizontalRecentering.m_WaitTime = 3;
        POV.m_VerticalRecentering.m_RecenteringTime = 10;
        POV.m_VerticalRecentering.m_WaitTime = 3;
        toiletCam.LookAt = portrait.transform;
        yield return new WaitForSeconds(1);
        //TODO: Make sounds play when not looking at it
        GameManager.instance.FadeIn();
        inspectWindow.SetActive(true);
        while (!HasWindowBeenInspected) yield return null;
        yield return new WaitForSeconds(1);
        risingTensionAudio.Play();
        yield return new WaitForSeconds(3.5f);
        POV.m_HorizontalRecentering.m_enabled = true;
        POV.m_VerticalRecentering.m_enabled = true;
        float timer = 0;
        while (timer < 24f)
        {
            POV.m_HorizontalRecentering.m_RecenteringTime = Mathf.Lerp(startRecenter, finishRecenter, timer / 24);
            POV.m_VerticalRecentering.m_RecenteringTime = Mathf.Lerp(startRecenter, finishRecenter, timer / 24);
            POV.m_HorizontalRecentering.m_WaitTime = Mathf.Lerp(startWait, finishWait, timer / 24);
            POV.m_VerticalRecentering.m_WaitTime = Mathf.Lerp(startWait, finishWait, timer / 24);
            timer += Time.deltaTime;
            yield return null;
        }
        //yield return new WaitForSeconds(2);

        doorOpen.Play();
        yield return new WaitForSeconds(3);
        running.Play();
        yield return new WaitForSeconds(.5f);
        PortraitManager.instance.SetPhase(2);
        PortraitManager.instance.InstantlyStopAllEffects();
        POV.m_HorizontalRecentering.m_enabled = false;
        POV.m_VerticalRecentering.m_enabled = false;
        huh.Play();
        yield return new WaitForSeconds(6.5f);
        doorOpen.Play();
        GameManager.instance.FadeOut();
        yield return new WaitForSeconds(1);
        toiletCam.Priority = 0;
        yield return new WaitForSeconds(1);
        GameManager.instance.FadeIn();
        HasTakenShit = true;
        basementDoor.ChangeMessage("Was someone in here?");
        //TODO: Put portrait back, open front door, set objects to right state
        onFrontDoor.SetActive(true);
        offFrontDoor.SetActive(false);
        GameManager.instance.IsShitting = false;
    }
}
