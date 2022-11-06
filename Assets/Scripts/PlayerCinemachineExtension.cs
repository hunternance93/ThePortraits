using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCinemachineExtension : CinemachineExtension
{
    private GameObject _player = null;
    private CinemachinePOV _pov = null;

    private GameObject Player
    {
        get
        {
            if (_player == null)
            {
                _player = VirtualCamera.Follow.gameObject.GetComponentInParent<Player>().gameObject;
            }

            return _player;
        }
    }

    private CinemachinePOV POV
    {
        get
        {
            if (_pov == null)
            {
                _pov = ((CinemachineVirtualCamera)VirtualCamera).GetCinemachineComponent<CinemachinePOV>();
            }
            
            return _pov;
        }
    }
    

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (VirtualCamera.name == "ToiletCamera") return;
        if (stage == CinemachineCore.Stage.Finalize)
        {
            if (CinemachineCore.Instance.IsLive(vcam))
            {
                // Rotate Kaede to the camera's angle.
                Player.transform.eulerAngles = new Vector3(0, state.RawOrientation.eulerAngles.y, 0);
            }
        }
    }
}
