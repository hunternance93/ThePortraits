using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFallTrigger : MonoBehaviour
{
    public Rigidbody[] rocksToFall = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (Rigidbody rb in rocksToFall)
            {
                rb.useGravity = true;
            }
            GetComponent<Collider>().enabled = false;
        }
    }
}
