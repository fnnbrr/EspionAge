using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectivePointer : Singleton<WorldObjectivePointer>
{
    public GameObject pointer;
    public float rotationSmoothness = 5f;

    private bool isPointing = false;
    private Vector3 currentPointPosition;
    private RegionTrigger waitForRegion;

    private void Awake()
    {
        Deactivate();
    }

    private void FixedUpdate()
    {
        if (isPointing)
        {
            Quaternion targetRotation = Quaternion.RotateTowards(
                pointer.transform.rotation,
                LookRotation(currentPointPosition),
                rotationSmoothness);

            Vector3 onlyYRotation = new Vector3(0f, targetRotation.eulerAngles.y, 0f);

            pointer.transform.rotation = Quaternion.Euler(onlyYRotation);
        }
    }

    private Quaternion LookRotation(Vector3 worldPosition)
    {
        return Quaternion.LookRotation(worldPosition - pointer.transform.position);
    }

    public void PointTo(Vector3 worldPosition, RegionTrigger stopOnEnterRegion = null)
    {
        currentPointPosition = worldPosition;
        // Set the current rotation to be directly opposite from our desired rotation, this gives a nice rotation effect on display
        pointer.transform.rotation = LookRotation(currentPointPosition) * Quaternion.Euler(0, 180f, 0);
        Activate();

        if (stopOnEnterRegion)
        {
            waitForRegion = stopOnEnterRegion;
            RegionManager.Instance.OnPlayerEnterRegion += HandlePlayerEnterRegion;
        }
    }

    private void HandlePlayerEnterRegion(RegionTrigger region)
    {
        if (region == waitForRegion)
        {
            Deactivate();
        }
    }

    private void Activate()
    {
        isPointing = true;
        pointer.SetActive(true);
    }

    private void Deactivate()
    {
        isPointing = false;
        pointer.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, currentPointPosition);
    }
}
