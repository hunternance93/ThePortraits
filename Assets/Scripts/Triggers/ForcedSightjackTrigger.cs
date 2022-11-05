using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedSightjackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerCameraSwitcher playerCameraSwitcher = null;
    [SerializeField] private GameObject cameraToForce = null;
    [SerializeField] private float sightJackLength = 3;
    [SerializeField] private float extraBuildUpTime = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(playerCameraSwitcher.ForceSightjack(sightJackLength, cameraToForce, extraBuildUpTime));
        }
    }
}
