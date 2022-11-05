using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    public Transform objectToFace = null;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.instance.CaughtPlayer(objectToFace == null ? transform : objectToFace);
        }
    }
}
