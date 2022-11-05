using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeTrigger : MonoBehaviour
{
    public float intensity = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.instance.PermanantlyShakeCameras(intensity);
            GetComponent<Collider>().enabled = false;
        }
    }
}
