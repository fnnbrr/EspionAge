using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZone : MonoBehaviour
{
    public CinemachineVirtualCamera mainCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            CameraManager.Instance.BlendTo(mainCamera);
        }
    }
}
