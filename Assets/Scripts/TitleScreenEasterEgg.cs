using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenEasterEgg : MonoBehaviour
{
    public GameObject NormalEnemy = null;
    public GameObject EggEnemy = null;
    public Transform AltEnemyTarget = null;
    public Image Emoji = null;
    public Sprite AltEmoji = null;

    void Start()
    {
        if (PlayerPrefs.HasKey("GameCompleted") && PlayerPrefs.GetInt("GameCompleted") == 1)
        {
            if (Random.Range(1, 1001) == 1)
            {

                NormalEnemy.SetActive(false);
                EggEnemy.SetActive(true);
                StartCoroutine(WaitAFrameForBuuku());
            }
            if (Random.Range(1, 101) == 1)
            {
                Emoji.sprite = AltEmoji;
            }
        }
    }

    private IEnumerator WaitAFrameForBuuku()
    {
        yield return null;
        EnemyAI EggEnemyAI = EggEnemy.GetComponent<EnemyAI>();
        EggEnemyAI.lastKnownTargetPosToBeUpdated = AltEnemyTarget.position;
        EggEnemyAI.lastKnownTargetPos = AltEnemyTarget.position;
        EggEnemyAI.ForceChasing = true;
        EggEnemyAI.SetState(EnemyAI.EnemyState.Chasing);
    }
}
