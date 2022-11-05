using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLookAxisProvider : MonoBehaviour, AxisState.IInputAxisProvider
{
    public float threshold = 15;

    private void Start()
    {
        if (Mathf.Sign(threshold) == -1)
        {
            threshold = -threshold;
        }
    }

    public float GetAxisValue(int axis)
    {
        if (axis > 1) return 0;

        Vector2 input = GameManager.instance.controls.PlayerControl.CameraLook.ReadValue<Vector2>();

        if (axis == 0)
        {
            return (threshold == 0) ? input.x : Mathf.Clamp(input.x, -threshold, threshold);
        }
        
        return (threshold == 0) ? input.y : Mathf.Clamp(input.y, -threshold, threshold);
    }
}
