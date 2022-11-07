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

    [HideInInspector] public bool HasWindowBeenInspected = false;
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
        StartCoroutine(TransitionToToiletCam()); //todo delte
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

    private IEnumerator TransitionToToiletCam()
    {
        GameManager.instance.FadeOut();
        //GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
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
        yield return new WaitForSeconds(5);
        POV.m_HorizontalRecentering.m_enabled = true;
        POV.m_VerticalRecentering.m_enabled = true;
        float timer = 0;
        while (timer < 30)
        {
            POV.m_HorizontalRecentering.m_RecenteringTime = Mathf.Lerp(startRecenter, finishRecenter, timer / 30);
            POV.m_VerticalRecentering.m_RecenteringTime = Mathf.Lerp(startRecenter, finishRecenter, timer / 30);
            POV.m_HorizontalRecentering.m_WaitTime = Mathf.Lerp(startWait, finishWait, timer / 30);
            POV.m_VerticalRecentering.m_WaitTime = Mathf.Lerp(startWait, finishWait, timer / 30);
            timer += Time.deltaTime;
            yield return null;
        }

        //TODO: Make big sound play, player goes what was dat, player gets control back, open front door, put portrait back
    }
}
