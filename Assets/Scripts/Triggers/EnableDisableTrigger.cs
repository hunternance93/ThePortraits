using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableTrigger : MonoBehaviour
{
    public bool DisableObjectsOnEnter = true;
    public GameObject[] ObjectList = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        foreach (GameObject go in ObjectList)
        {
            go.SetActive(!DisableObjectsOnEnter);
        }
    }
}
