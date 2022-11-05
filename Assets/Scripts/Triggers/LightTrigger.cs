using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log("Light parent and parent's parent: " + transform.parent.gameObject + transform.parent.parent.gameObject);
            GameManager.instance.Player.updateLightCount(1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.instance.Player.updateLightCount(-1);
        }
    }
}
