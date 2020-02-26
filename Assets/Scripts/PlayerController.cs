using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public float baseMovementSpeed = 5f;
    public float baseTurnSpeed = 2f;
    public float canMoveRotationThreshold = 0.1f;
    public float consideredMovementThreshold = 0.1f;
    
    [HideInInspector] public float movementSpeed;
    [HideInInspector] public float turnSpeed;
    private Rigidbody rb;

    private Vector3 movement;
    public bool EnablePlayerInput { get; set; } = true;

    void Start()
    {
        movementSpeed = baseMovementSpeed;
        turnSpeed = baseTurnSpeed;
        rb = Utils.GetRequiredComponent<Rigidbody>(this);

        if (CameraManager.Instance)
        {
            CameraManager.Instance.OnBlendingStart += HandleCameraOnBlendingStart;
            CameraManager.Instance.OnBlendingComplete += HandleCameraOnBlendingComplete;
        }
    }

    void FixedUpdate()
    {
        if (!EnablePlayerInput)
        {
            return;
        }

        // early exit, since we cannot do relative-to-camera player movement otherwise
        if (!CameraManager.Instance.IsActiveCameraValid())
        {
            return;
        }

        Vector3 relativeForward = CleanForwardVector(CameraManager.Instance.GetActiveCameraTransform().forward);
        Vector3 relativeRight = CalculateRightVector(relativeForward);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movement = relativeForward * vertical + relativeRight * horizontal;
        movement.Normalize();
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;    // to be used later for animations and such

        HandleControl(movement);
    }

    private Vector3 CleanForwardVector(Vector3 forwardVector)
    {
        forwardVector.y = 0f;
        return forwardVector.normalized;
    }

    private Vector3 CalculateRightVector(Vector3 forwardVector)
    {
        // The vector perpendicular to the forward vector and this transform's up, must be the relative right vector
        return -Vector3.Cross(forwardVector, transform.up).normalized;
    }

    void HandleControl(Vector3 movementDirection)
    {
        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, movementDirection, turnSpeed * Time.fixedDeltaTime, 0f);

        HandleRotation(desiredForward);

        // Only start moving once we are close enough to our desired final rotation (which we are smoothly rotating towards)
        if ((movementDirection - desiredForward).magnitude < canMoveRotationThreshold)
        {
            HandleMovement();
        }
    }

    void HandleRotation(Vector3 desiredForward)
    {
        rb.MoveRotation(Quaternion.LookRotation(desiredForward));
    }

    void HandleMovement()
    {
        rb.MovePosition(rb.position + movement * (movementSpeed * Time.fixedDeltaTime));
    }

    private void HandleCameraOnBlendingStart(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        EnablePlayerInput = false;
    }

    private void HandleCameraOnBlendingComplete(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        EnablePlayerInput = true;
    }
}
