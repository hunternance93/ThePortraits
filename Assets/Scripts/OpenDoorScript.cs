using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Common;

public class OpenDoorScript : MonoBehaviour
{
    public float TargetAngle = 90;
    public float OtherTargetAngle = 0;
    public float TimeToOpen = 1;
    public float RotationThreshold = .01f;

    [Tooltip("Weird roundabout fix for issue where mesh collider for new door object can be wrong. If issue happens assign it here")]
    public MeshCollider toggleCollider = null;

    [Tooltip("(Optional) Unique ID for if you want this door to stay unlocked even if player dies / reloads scene. Leave blank to not have this happen.")]
    public string SaveDoorOpenState = "";
    [Tooltip("(Optional) Objects to delete if this door is opened on load (interactable components)")]
    public GameObject[] objectsToDelete = null;
    [Tooltip("(Optional) Objects to enable if this door is opened on load (interactable components)")]
    public GameObject[] objectsToEnable = null;

    private Coroutine isInMotion = null;

    public bool IsOpen = false;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(SaveDoorOpenState)) {
            if (SecureSaveFile.Instance.GetInt(SaveDoorOpenState, 0) == 1)
            {
                OpenDoorInstant();
                DeleteObjectsOnLoadOpen();
                ObjectsToEnableOnLoadOpen();
            }
        }
    }

    private void DeleteObjectsOnLoadOpen()
    {
        foreach (GameObject g in objectsToDelete)
        {
            if (g != null) Destroy(g);
        }
    }

    private void ObjectsToEnableOnLoadOpen()
    {
        foreach (GameObject g in objectsToEnable)
        {
            g.SetActive(true);
        }
    }

    public void OpenDoor()
    {
        if (isInMotion != null) return;
        if (!IsOpen)
        {
            IsOpen = true;
            isInMotion = StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            IsOpen = false;
            isInMotion = StartCoroutine(CloseDoorRoutine());
        }
    }

    public void ReverseOpenDirection()
    {
        TargetAngle *= -1;
    }

    public void OpenDoorInstant()
    {
        transform.localEulerAngles = new Vector3(transform.rotation.x, TargetAngle, transform.rotation.z);
    }

    public IEnumerator OpenDoorRoutine()
    {
        if (toggleCollider != null)
        {
            toggleCollider.enabled = false;
        }
        float timer = 0;
        while (timer < TimeToOpen) {
            timer += Time.deltaTime;
            transform.localEulerAngles = new Vector3(transform.rotation.x, Mathf.Lerp(OtherTargetAngle, TargetAngle, timer/TimeToOpen), transform.rotation.z);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(transform.rotation.x, TargetAngle, transform.rotation.z);
        if (toggleCollider != null)
        {
            toggleCollider.enabled = true;
        }
        isInMotion = null;
    }

    public IEnumerator CloseDoorRoutine()
    {
        if (toggleCollider != null)
        {
            toggleCollider.enabled = false;
        }
        float timer = 0;
        while (timer < TimeToOpen)
        {
            timer += Time.deltaTime;
            transform.localEulerAngles = new Vector3(transform.rotation.x, Mathf.Lerp(TargetAngle, OtherTargetAngle, timer / TimeToOpen), transform.rotation.z);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(transform.rotation.x, OtherTargetAngle, transform.rotation.z);
        if (toggleCollider != null)
        {
            toggleCollider.enabled = true;
        }
        isInMotion = null;
    }
}
