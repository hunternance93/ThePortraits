using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProTipTrigger : MonoBehaviour
{

    [SerializeField] private string proTipMessage = "";
    [SerializeField] private float proTipMessageLength = 10;
    [SerializeField] private float delayMessageTimer = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (!string.IsNullOrEmpty(proTipMessage))
            {
                if (delayMessageTimer > 0)
                {
                    StartCoroutine(WaitThenMessage());
                }
                else
                {
                    GameManager.instance.DisplayProTip(proTipMessage, proTipMessageLength);
                    GetComponent<BoxCollider>().enabled = false;
                }
            }
            
        }
    }

    private IEnumerator WaitThenMessage()
    {
        yield return new WaitForSeconds(delayMessageTimer);
        GameManager.instance.DisplayProTip(proTipMessage, proTipMessageLength);
        GetComponent<BoxCollider>().enabled = false;
    }
}
