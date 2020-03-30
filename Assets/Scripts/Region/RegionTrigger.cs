using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionTrigger : MonoBehaviour
{
    public delegate void PlayerPassedThroughAction();
    public event PlayerPassedThroughAction OnPlayerPassThrough;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerEnterRegion(this);
        }
        else if (other.gameObject.HasComponentInChildren<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectEnterRegion(other.gameObject.GetComponentInChildren<LocationTrackable>().gameObject, this);
        }
        else if (other.gameObject.HasComponentInParent<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectEnterRegion(other.gameObject.GetComponentInParent<LocationTrackable>().gameObject, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerExitRegion(this);
            OnPlayerPassThrough?.Invoke();
        }
        else if (other.gameObject.HasComponentInChildren<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectExitRegion(other.gameObject.GetComponentInChildren<LocationTrackable>().gameObject, this);
        }
        else if (other.gameObject.HasComponentInParent<LocationTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectExitRegion(other.gameObject.GetComponentInParent<LocationTrackable>().gameObject, this);
        }
    }
}
