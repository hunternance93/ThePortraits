using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightjackingListChangeTrigger : MonoBehaviour
{
    public GameObject[] SightJackCams;
    public GameObject visionCam;

    public float extraBuildUpToVision = 0;
    public bool EmpoweredHeirloomScene = false;

    private bool showSightJackChangeCamera = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (SightJackCams.Length == 0) { 
                GameManager.instance.Player.SetSightJackCams(new GameObject[0]);
                return;
            }
            if (!GameManager.instance.Player.SightJackCamsAre(SightJackCams))
            {
                if (showSightJackChangeCamera)
                {
                    GameManager.instance.Player.IndicateNewSightjackCams(visionCam, extraBuildUpToVision, EmpoweredHeirloomScene);
                    showSightJackChangeCamera = false;
                }
                GameManager.instance.Player.SetSightJackCams(SightJackCams);
            }
        }
    }
}
