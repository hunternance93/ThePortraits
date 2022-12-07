using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToPlayerScript : MonoBehaviour
{
    void Update()
    {
        Debug.Log("this happenin");
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
    }
}
