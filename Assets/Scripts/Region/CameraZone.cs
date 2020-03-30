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
        else if (other.gameObject.HasComponentInChildren<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectEnterZone(other.gameObject.GetComponentInChildren<LocationTrackable>().gameObject, this);
        }
        else if (other.gameObject.HasComponentInParent<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectEnterZone(other.gameObject.GetComponentInParent<LocationTrackable>().gameObject, this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerExitZone(this);
        }
        else if (other.gameObject.HasComponentInChildren<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectExitZone(other.gameObject.GetComponentInChildren<LocationTrackable>().gameObject, this);
        }
        else if (other.gameObject.HasComponentInParent<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectExitZone(other.gameObject.GetComponentInParent<LocationTrackable>().gameObject, this);
        }
    }
}
