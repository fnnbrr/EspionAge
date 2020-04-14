using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectivePointer : Singleton<WorldObjectivePointer>
{
    public GameObject pointer;
    public float rotationSmoothness = 5f;

    private bool isPointing = false;
    private Vector3 currentPointPosition;
    private Transform currentPointTransform;
    private RegionTrigger waitForRegion;

    private enum PointType
    {
        Position,
        Transform
    }
    private PointType pointType;

    private void Awake()
    {
        Deactivate();
    }

    private void FixedUpdate()
    {
        if (isPointing)
        {
            Vector3 pointPosition = currentPointPosition;
            switch (pointType)
            {
                case PointType.Transform:
                    pointPosition = currentPointTransform.position;
                    break;
            }

            Quaternion targetRotation = Quaternion.RotateTowards(
                pointer.transform.rotation,
                LookRotation(pointPosition),
                rotationSmoothness);

            Vector3 onlyYRotation = new Vector3(0f, targetRotation.eulerAngles.y, 0f);

            pointer.transform.rotation = Quaternion.Euler(onlyYRotation);
        }
    }

    private Quaternion LookRotation(Vector3 worldPosition)
    {
        return Quaternion.LookRotation(worldPosition - pointer.transform.position);
    }

    public void PointTo(Transform pointToTransform, Interactable stopOnEndInteract = null)
    {
        currentPointTransform = pointToTransform;
        pointType = PointType.Transform;
        LookOppositeToPosition(pointToTransform.position);
        Activate();

        if (stopOnEndInteract)
        {
            stopOnEndInteract.OnInteractEnd += HandleInteractEnd;
        }
    }

    private void HandleInteractEnd(Interactable source)
    {
        Deactivate();
    }

    public void PointTo(Vector3 worldPosition, RegionTrigger stopOnEnterRegion = null)
    {
        currentPointPosition = worldPosition;
        pointType = PointType.Position;
        LookOppositeToPosition(currentPointPosition);
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

    private void LookOppositeToPosition(Vector3 position)
    {
        // Set the current rotation to be directly opposite from our desired rotation, this gives a nice rotation effect on display
        pointer.transform.rotation = LookRotation(position) * Quaternion.Euler(0, 180f, 0);
    }

    private void Activate()
    {
        isPointing = true;
        pointer.SetActive(true);
    }

    public void Deactivate()
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
