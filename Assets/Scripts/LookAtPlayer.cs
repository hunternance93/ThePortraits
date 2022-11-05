using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public bool PursuePlayer = false;
    public float PursueDistance = 100;
    public Transform alternateTarget = null;
    private Transform playerTransform = null;
    private Player playerRef = null;
    [SerializeField] private Transform[] positionToTeleportToOnCheckpoint;

    [SerializeField] private float finalCollapseHeight = 60;
    [SerializeField] private float collapseLengthInSeconds = 360;

    [SerializeField] private AudioSource idleNoise = null;
    [SerializeField] private AudioSource chaseNoise = null;

    private const float pursueBuffer = 5;

    private Vector3 startingPos;

    private bool collapsing = false;
    private float collapseTimer = 0;
    private float startingPursueDistance;

    private bool verySoftShaking = false;
    private bool softShaking = false;
    private bool fairlyHeavyShaking = false;
    private bool heavyShaking = false;

    // Start is called before the first frame update
    void Start()
    {
        if (startingPos == new Vector3(0,0,0)) startingPos = transform.position;
        if (alternateTarget != null) playerTransform = alternateTarget;
        else
        {
            playerRef = GameManager.instance.Player;
            playerTransform = playerRef.transform;
        }

        if (GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore) collapseLengthInSeconds -= 60;
    }

    public void BeginCollapse()
    {
        collapsing = true;
        startingPursueDistance = PursueDistance;
        //TODO: Add roar, change animation
        Debug.Log("Collapse begins");
        idleNoise.Stop();
        chaseNoise.Play();
        GetComponent<Animator>().SetBool("ChaseLoop", true);
    }

    public void SetToPosition(int index)
    {
        if (startingPos == new Vector3(0, 0, 0)) startingPos = transform.position;
        if (index < positionToTeleportToOnCheckpoint.Length) transform.position = positionToTeleportToOnCheckpoint[index].position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(playerTransform.position);

        if (collapsing)
        {
            collapseTimer += Time.deltaTime;
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(startingPos.y, finalCollapseHeight, collapseTimer / collapseLengthInSeconds), transform.position.z);
            PursueDistance = Mathf.Lerp(startingPursueDistance, 15, collapseTimer / collapseLengthInSeconds);

            if (!verySoftShaking && collapseTimer >= collapseLengthInSeconds / 4)
            {
                verySoftShaking = true;
                BeginVerySoftShaking();
            }
            else if (!softShaking && collapseTimer >= collapseLengthInSeconds/2)
            {
                softShaking = true;
                BeginSoftShaking();
            }
            else if (!fairlyHeavyShaking && !GameManager.instance.isCameraShaking && collapseTimer >= collapseLengthInSeconds * 3 / 4)
            {
                fairlyHeavyShaking = true;
                BeginFairlyHeavyShaking();
            }
            else if (!heavyShaking && !GameManager.instance.isCameraShaking && collapseTimer >= collapseLengthInSeconds * 9 / 10)
            {
                heavyShaking = true;
                BeginHeavyShaking();
            }

            if (collapseTimer >= collapseLengthInSeconds)
            {
                if (GameManager.instance.CurrentGameMode != GameManager.GameMode.Story && GameManager.instance.CurrentGameMode != GameManager.GameMode.DevCommentary && !GameManager.instance.GameEnding.IsGameOver) GameManager.instance.CaughtPlayer(transform);
                else if (GameManager.instance.CurrentGameMode == GameManager.GameMode.DevCommentary)
                {
                    collapseTimer = 0;
                    StopShaking();
                    verySoftShaking = false;
                    softShaking = false;
                    fairlyHeavyShaking = false;
                    heavyShaking = false;
                }
            }
                    
        }

        if (PursuePlayer)
        {
            Vector3 tarPos = new Vector3(GameManager.instance.Player.transform.position.x, transform.position.y, GameManager.instance.Player.transform.position.z);
            float currentDistance = Vector3.Distance(transform.position, tarPos);        
            Vector3 dir = Vector3.Normalize(tarPos - transform.position);      
            transform.position += dir * (currentDistance - PursueDistance);
        }
    }

    private void BeginVerySoftShaking()
    {
        Debug.Log("Begin Very Soft Shaking");
        GameManager.instance.PermanantlyShakeCameras(.25f);
    }

    private void BeginSoftShaking()
    {
        Debug.Log("Begin Soft Shaking");
        GameManager.instance.PermanantlyShakeCameras(1);
    }

    private void BeginFairlyHeavyShaking()
    {
        Debug.Log("Begin Fairly Heavy Shaking");
        GameManager.instance.PermanantlyShakeCameras(2.5f);
    }

    private void BeginHeavyShaking()
    {
        Debug.Log("Begin Heavy Shaking");
        GameManager.instance.PermanantlyShakeCameras(5);
    }

    private void StopShaking()
    {
        Debug.Log("Stop Shaking");
        GameManager.instance.PermanantlyShakeCameras(0);
    }
}
