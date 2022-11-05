using UnityEngine;

//Script used to begin an enemy's behavior when player enters it
public class EnemyTriggerBox : MonoBehaviour
{
    [SerializeField] EnemyAI enemyToTrigger = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            enemyToTrigger.TriggerStart();
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
