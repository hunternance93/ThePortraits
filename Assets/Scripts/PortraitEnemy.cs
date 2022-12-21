using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitEnemy : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.instance.GameEnding.CaughtPlayer(transform);
        }
    }
}
