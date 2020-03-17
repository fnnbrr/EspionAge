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
        else if (other.gameObject.HasComponent<RegionTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectEnterRegion(other.gameObject, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            RegionManager.Instance.ReportPlayerExitRegion(this);
            OnPlayerPassThrough?.Invoke();
        }
        else if (other.gameObject.HasComponent<RegionTrackable>())
        {
            RegionManager.Instance.ReportTrackedObjectExitRegion(other.gameObject, this);
        }
    }
}
