using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LadderInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Message displayed if player has enemy aggro and can't use this ladder")]
    [SerializeField] private string aggroFailureMessage = "It's too dangerous to do that right now";
    [Tooltip("How long failure messages should display")]
    [SerializeField] private float messageLength = 1.5f;
    [Tooltip("Canvas fader to fade")]
    [SerializeField] private CanvasFadeOut fade = null;
    [Tooltip("Game object of the ladder camera")]
    [SerializeField] private GameObject ladderCamera = null;
    [Tooltip("Used to move enemy's position when player uses ladder")]
    [SerializeField] private EnemyAI enemyObject = null;
    [Tooltip("Location to move enemy position to when ladder used")]
    [SerializeField] private Transform positionToMoveTo = null;
    [Tooltip("Objects to enable on ladder usage (enemies in resi and onward to save resources")]
    [SerializeField] private GameObject[] objsToEnable = null;
    [SerializeField] private AudioSource audToStop = null;

    private bool enemyObjNotMoved = true;
    private Quaternion initRot;

    private void Start()
    {
        initRot = enemyObject.transform.rotation;
    }

    public void Interacted()
    {
        HandleLadder();
    }

    private void HandleLadder()
    {
        if (GameManager.instance.Player.EnemyAggroCount > 0)
        {
            GameManager.instance.DisplayMessage(aggroFailureMessage, messageLength);
            return;
        }

        MoveEnemy();

        foreach(GameObject go in objsToEnable)
        {
            go.SetActive(true);
        }

        audToStop.Stop();

        GameManager.instance.Player.CanSightJack = false;
        AudioManager.instance.PlayDiscordantOrchestra();
        StartCoroutine(FadeOutAndSwitchCameras());
    }

    public void MoveEnemy()
    {
        if (enemyObjNotMoved && enemyObject != null)
        {
            enemyObjNotMoved = false;
            if (enemyObject.GetState() != EnemyAI.EnemyState.Searching) enemyObject.SetState(EnemyAI.EnemyState.Searching);
            enemyObject.ResetRotationValues();

            //A lot of bullshit to make sure that the potentially "revived" enemy is in sightjack list
            if (enemyObject.IsDisabledStory())
            {
                enemyObject.SetDisabledStory(false);
                GameObject childFOVObj = enemyObject.GetComponentInChildren<FieldOfView>().gameObject;
                GameObject[] sightjacks = GameManager.instance.Player.PCS.GhostPOVs;
                if (sightjacks[0] != childFOVObj)
                {
                    GameObject[] tempArray = new GameObject[sightjacks.Length + 1];
                    tempArray[0] = childFOVObj;
                    for(int i = 0; i < sightjacks.Length; i++)
                    {
                        tempArray[i + 1] = sightjacks[i];
                    }
                    sightjacks = tempArray;
                }
                GameManager.instance.Player.SetSightJackCams(sightjacks);
            }
            enemyObject.transform.position = positionToMoveTo.position;
            enemyObject.transform.rotation = initRot;
        }
    }

    private IEnumerator FadeOut()
    {
        StartCoroutine(fade.FadeOut());
        yield return new WaitForSeconds(fade.FadeLength);
    }
    
    private IEnumerator FadeOutAndSwitchCameras()
    {
        yield return FadeOut();
        GameManager.instance.Player.gameObject.SetActive(false);
        ladderCamera.SetActive(true);
    }
}
