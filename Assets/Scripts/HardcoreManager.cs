using UnityEngine;

public class HardcoreManager : MonoBehaviour
{
    public GameObject HardCoreObjects = null;

    public EnemyAI[] enemiesToModify;
    public float[] patrolSpeedChanges;
    public float[] chaseSpeedChanges;

    public EnemyAI enemyToAwaken = null;

    public GameObject[] ObjectsToDelete = null;

    void Start()
    {
        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore)
        {
            if (HardCoreObjects != null) HardCoreObjects.SetActive(true);

            for (int i = 0; i < enemiesToModify.Length - 1; i++)
            {
                enemiesToModify[i].ModifySpeeds(patrolSpeedChanges[i], chaseSpeedChanges[i]);
            }

            if (enemyToAwaken != null)
            {
                enemyToAwaken.SleepingEnemy = false;
                enemyToAwaken.IsCurrentlySleeping = false;
            }

            foreach(GameObject go in ObjectsToDelete)
            {
                if (go != null) Destroy(go);
            }
        }
    }
}
