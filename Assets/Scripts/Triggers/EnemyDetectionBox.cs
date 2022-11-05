using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionBox : MonoBehaviour
{
    [SerializeField] EnemyAI[] enemiesToDetect = null;
    [Tooltip("Optional: If this is not null, then the enemy will head towards this as its target")]
    [SerializeField] Transform target = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (EnemyAI enemyToDetect in enemiesToDetect)
            {
                if (!enemyToDetect.IsDisabledStory())
                {
                    if (target != null)
                    {
                        enemyToDetect.lastKnownTargetPosToBeUpdated = target.position;
                        enemyToDetect.lastKnownTargetPos = target.position;
                    }
                    else
                    {
                        enemyToDetect.lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerHead.position;
                        enemyToDetect.lastKnownTargetPos = GameManager.instance.Player.PlayerHead.position;
                    }
                    enemyToDetect.ForceChasing = true;
                    enemyToDetect.SetState(EnemyAI.EnemyState.Chasing);
                    GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
    }
}
