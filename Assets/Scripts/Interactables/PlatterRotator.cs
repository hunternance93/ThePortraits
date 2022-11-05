using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatterRotator : MonoBehaviour { 

    public float AngleToRotate = 90f;
    public float RotationTime = 2.65f;
    public bool startsAnimated = false;
    public int solutionPosition = 0;

    public GameObject WaterSource = null;

    [SerializeField] private GameObject[] solvedItems = null;
    [SerializeField] private OpenDoorScript[] doorsToOpen;
    [SerializeField] private GameObject[] objectsToDisableOnSolve;
    [SerializeField] private AudioSource[] soundsToPlayOnSolve;
    [SerializeField] private PlatterRotator[] subsequentRotators;
    [SerializeField] private PourDetector shishi;
    [SerializeField] private float doorDelay = 0.3f;

    private Animator shishiAnimator = null;

    private Quaternion startAngle;
    private Quaternion endAngle;
    private float rotationProgress = -1;
    private bool shouldPlaySound = true;
    public int currentPosition = 0;

    public void Start()
    {
        shishiAnimator = GetComponentInChildren<Animator>();

        if (!startsAnimated)
        {
            shishiAnimator.enabled = false;
        }
        else
        {
            shishiAnimator.SetBool("isTipping", true);
        }
        currentPosition = 0;
    }

    public void Update ()
    {
        if (rotationProgress > -1 && rotationProgress < RotationTime)
        {
            rotationProgress += Time.deltaTime;

            transform.rotation = Quaternion.Lerp(startAngle, endAngle, rotationProgress/RotationTime);
        } 
        if (rotationProgress >= RotationTime)
        {
            currentPosition = (currentPosition + 1) % 4;
            CheckForSolution();
            rotationProgress = -1;
        }
    }

    private IEnumerator OpenDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (OpenDoorScript door in doorsToOpen)
        {
            door.OpenDoor();
        }
    }

    public void CheckForSolution()
    {
        if (currentPosition == solutionPosition && WaterSource.activeSelf)
        {
            shishiAnimator.enabled = true;
            shishiAnimator.SetBool("isTipping", true);
            foreach (GameObject solvedItem in solvedItems)
            {
                solvedItem.SetActive(true);
            }
            foreach (GameObject go in objectsToDisableOnSolve)
            {
                go.SetActive(false);
            }
            foreach (AudioSource sound in soundsToPlayOnSolve)
            {
                if (shouldPlaySound)
                {
                    sound.Play();
                    shouldPlaySound = false;
                }
            }
            foreach(PlatterRotator pr in subsequentRotators)
            {
                pr.CheckForSolution();
            }
            StartCoroutine(OpenDoorAfterDelay(doorDelay));

        }
        else
        {
            if (shishiAnimator.isActiveAndEnabled)
            {
                shishiAnimator.SetBool("isTipping", false);
            }
            foreach (GameObject solvedItem in solvedItems)
            {
                solvedItem.SetActive(false);
            }
            foreach(PlatterRotator pr in subsequentRotators)
            {
                pr.CheckForSolution();
            }
        }
    }

    public void Activate ()
    {
        RotatePlatform();
    }

    private void RotatePlatform()
    {
        startAngle = transform.rotation;
        endAngle = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 90f, transform.rotation.eulerAngles.z);

        rotationProgress = 0;
    }
}
