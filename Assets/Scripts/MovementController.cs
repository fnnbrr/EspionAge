using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMovementSpeed = 5f;
    public float baseTurnSpeed = 2f;
    public float canMoveRotationThreshold = 0.1f;
    public float consideredMovementThreshold = 0.1f;
    
    [HideInInspector] public float movementSpeed;
    [HideInInspector] public float turnSpeed;
    private Rigidbody rb;
    private Animator anim;
    private Vector3 movement;
    private Vector3 currentMovementAxis;
    private bool currentMovementAxisInitialized = false;

    //[FMODUnity.EventRef]
    //public string boost;

    [Header("Special / Dash")]
    public float dashForce;
    [Range(0f, StaminaBar.FILL_MAX)] public float specialAwakenessDecrease = 0.5f;
    public List<TrailRenderer> specialTrailRenderers;

    public bool IsMoving { get; set; } = false;

    public delegate void MovementAction();
    public event MovementAction OnMovementChange;
    public event MovementAction OnStopMoving;

    void Start()
    {
        movementSpeed = baseMovementSpeed;
        turnSpeed = baseTurnSpeed;
        rb = Utils.GetRequiredComponent<Rigidbody>(this);
        anim = Utils.GetRequiredComponentInChildren<Animator>(this);

        ToggleSpecialTrailRenderers(false);

        OnMovementChange += UpdateMovementAxis;
    }

    private void UpdateMovementAxis()
    {
        if (currentMovementAxisInitialized && CameraManager.Instance.GetActiveCamera() != null)
        {
            currentMovementAxis = CameraManager.Instance.brain.transform.forward;
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;

        HandleSpecialInput();
    }

    private void LateUpdate()
    {
        // to resolve issues with Cinemachine initializing after Start: 
        //  https://forum.unity.com/threads/activevirtualcamera-is-null-not-sure-why.561541/
        if (!currentMovementAxisInitialized && CameraManager.Instance.GetActiveCamera() != null)
        {
            currentMovementAxis = CameraManager.Instance.GetActiveCameraTransform().forward;
            currentMovementAxisInitialized = true;
        }
    }

    private void HandleSpecialInput()
    {
        if (Input.GetButtonDown(Constants.INPUT_SPECIAL_GETDOWN) && UIManager.Instance.staminaBar.lightningActive)
        {
            StartCoroutine(EnableSpecialTrailRenderers(1f));
            PerformDash();
            UIManager.Instance.staminaBar.SetAwakeness(specialAwakenessDecrease);
        }
    }

    private IEnumerator EnableSpecialTrailRenderers(float forSeconds)
    {
        ToggleSpecialTrailRenderers(true);

        yield return new WaitForSeconds(forSeconds);

        ToggleSpecialTrailRenderers(false);
    }

    private void ToggleSpecialTrailRenderers(bool toggle)
    {
        specialTrailRenderers.ForEach(t =>
        {
            t.emitting = toggle;
        });
    }

    private void PerformDash()
    {
        //rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
        anim.SetTrigger(Constants.ANIMATION_BIRDIE_DASH);
        //FMODUnity.RuntimeManager.PlayOneShot(boost, transform.position);
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput)
        {
            SetIsWalking(false);
            return;
        }

        // early exit, since we cannot do relative-to-camera player movement otherwise
        if (!CameraManager.Instance.IsActiveCameraValid())
        {
            return;
        }

        Vector3 relativeForward = CleanForwardVector(currentMovementAxis);
        Vector3 relativeRight = CalculateRightVector(relativeForward);

        float horizontal = Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL);
        float vertical = Input.GetAxis(Constants.INPUT_AXIS_VERTICAL);

        SetIsMovement(relativeForward * vertical + relativeRight * horizontal);
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;    // to be used later for animations and such
        SetIsWalking(isWalking);

        HandleControl(movement);
    }

    public void SetIsWalking(bool isWalking)
    {
        if (IsMoving && !isWalking)
        {
            // Then we stopped walking
            OnStopMoving?.Invoke();
        }
        IsMoving = isWalking;
        anim.SetBool(Constants.ANIMATION_BIRDIE_ISWALKING, isWalking);
    }

    public void SetIsMovement(Vector3 newMovement)
    {
        if (movement != newMovement.normalized)
        {
            OnMovementChange?.Invoke();
        }
        movement = newMovement.normalized;  // always want to normalize
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
}