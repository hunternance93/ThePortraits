using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.Common;

public class OpenDoorScript : MonoBehaviour
{
    public float TargetAngle = 90;
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
        StartCoroutine(OpenDoorRoutine());
        if (!string.IsNullOrEmpty(SaveDoorOpenState)) {
            SecureSaveFile.Instance.SetInt(SaveDoorOpenState, 1);
            SecureSaveFile.Instance.SaveToDisk();
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
            transform.localEulerAngles = new Vector3(transform.rotation.x, Mathf.Lerp(0, TargetAngle, timer/TimeToOpen), transform.rotation.z);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(transform.rotation.x, TargetAngle, transform.rotation.z);
        if (toggleCollider != null)
        {
            toggleCollider.enabled = true;
        }
    }
}
