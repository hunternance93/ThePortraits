using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Set to false if you want to assign this spawnpoint by something else like picking up an item, or walking somewhere else")]
    [SerializeField] private bool triggerSaveOnEnter = true;
    [Tooltip("The angle the player should spawn in at")]
    [SerializeField] private float initialPlayerAngle = 0;
    [Tooltip("A list of objects that will be destroyed when player spawns here. Example: Earlier spawn points")]
    public GameObject[] ObjectsToDestroyOnLoad = null;
    [Tooltip("A list of objects that will be destroyed when player spawns here. Example: Earlier spawn points")]
    public GameObject[] ObjectsToActivateOnLoad = null;
    [Tooltip("A list of enemies to instantly move to their destination (for enemies that normally move from a trigger initially")]
    public EnemyAI[] EnemiesToMoveToDestination = null;
    [Tooltip("Used just to move the enemy that changes position when you touch the ladder when loading checkpoint")]
    public LadderInteractable LadderScript = null;
    [Tooltip("A list of doors to instantly open on start (for doors that should already be open by checkpoint)")]
    public UnlockableDoorInteractable[] DoorsToOpen = null;
    [Tooltip("A list of sightjack cameras to start with on this load. If same as default leave blank.")]
    public GameObject[] SightJackCams = null;
    [Tooltip("A list of enemies that should go opposite their normal patrol direction when the checkpoint is loaded")]
    public EnemyAI[] EnemiesToPatrolBackwards = null;
    [Tooltip("The amount of time the game waits before saying Game Saved in corner (good for visions)")]
    public float TimeDelayForGameSavedMessage = 0;
    [Tooltip("Only used for EscapeTown")]
    public LookAtPlayer bigHeadLAP = null;
    [Tooltip("Only used for EscapeTown")]
    public int bigHeadTarget = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerSaveOnEnter) return;
        //if hardcore return
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore) return;
            GameManager.instance.SaveGameManager.SaveGame(transform);
            if (TimeDelayForGameSavedMessage > 0) GameManager.instance.DisplaySaveTextAfterTime(TimeDelayForGameSavedMessage);
            else GameManager.instance.DisplaySaveText();
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void SaveCheckpoint()
    {
        SaveGameManager.Instance.SaveGame(transform);
        if (Application.isPlaying) GetComponent<BoxCollider>().enabled = false;
    }

    public void SpawnPlayer()
    {
        foreach (GameObject go in ObjectsToDestroyOnLoad)
        {
            if (go != null) Destroy(go);
        }
        foreach (GameObject go in ObjectsToActivateOnLoad)
        {
            go.SetActive(true);
        }
        foreach (EnemyAI enemy in EnemiesToMoveToDestination)
        {
            enemy.TriggerInstant();
        }
        foreach(UnlockableDoorInteractable door in DoorsToOpen)
        {
            door.OpenDoorInstantly();
        }
        foreach(EnemyAI enemy in EnemiesToPatrolBackwards)
        {
            enemy.SwapDirection();
        }
        if (LadderScript != null) LadderScript.MoveEnemy();
        if (SightJackCams != null && SightJackCams.Length > 0) GameManager.instance.Player.SetSightJackCams(SightJackCams);

        if (bigHeadLAP != null) bigHeadLAP.SetToPosition(bigHeadTarget);

        GetComponent<BoxCollider>().enabled = false;

        GameManager.instance.Player.FPSController.initialAngle = initialPlayerAngle;
        GameManager.instance.Player.SetPlayerAngle(initialPlayerAngle);
        GameManager.instance.Player.gameObject.transform.SetPositionAndRotation(transform.position, GameManager.instance.Player.gameObject.transform.rotation);
        
    }
}
