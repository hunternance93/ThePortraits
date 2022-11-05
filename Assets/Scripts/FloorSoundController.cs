using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSoundController : MonoBehaviour
{
    public enum FloorSoundType
    {
        Normal,
        Metal,
        Water,
        Tile,
        Wood,
        Tatami
    };

    public FloorSoundType DefaultTypeForScene = FloorSoundType.Normal;
    public float DistanceToCheck = 2f;


    private FloorSoundType currentFloorType = FloorSoundType.Normal;
    private LayerMask layerMask;

    void Start()
    {
        layerMask = LayerMask.GetMask("FloorSoundTile");
    }

    void Update()
    {
        FloorSoundType floorType = GetFloorSoundType();
        if (floorType != currentFloorType)
        {
            currentFloorType = floorType;
            AudioManager.instance.SetFootStepAudioClips(currentFloorType);
        }
    }

    private FloorSoundType GetFloorSoundType()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, DistanceToCheck, layerMask))
        {
            return DefaultTypeForScene;
        }
        try
        {
            return hit.transform.gameObject.GetComponent<FloorSoundTile>().Type;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return DefaultTypeForScene;
        }
    }
}
