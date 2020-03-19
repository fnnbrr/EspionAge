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
        //pointer.SetActive(false);
        PointTo(Vector3.zero);
    }

    private void FixedUpdate()
    {
        if (isPointing)
        {
            Quaternion targetRotation = Quaternion.RotateTowards(
                pointer.transform.rotation,
                Quaternion.LookRotation(currentPointPosition - pointer.transform.position),
                rotationSmoothness);

            Vector3 onlyYRotation = new Vector3(0f, targetRotation.eulerAngles.y, 0f);

            pointer.transform.rotation = Quaternion.Euler(onlyYRotation);
        }
    }

    public void PointTo(Vector3 worldPosition, RegionTrigger stopOnEnterRegion = null)
    {
        currentPointPosition = worldPosition;
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
