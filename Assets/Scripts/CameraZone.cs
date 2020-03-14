using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public class CameraZone : MonoBehaviour 
{
    public CinemachineVirtualCamera mainCamera;
    public string regionName;
    public bool isRestricted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerEnterZone(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerExitZone(this);
        }
    }
}
