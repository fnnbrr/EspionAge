using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMovementSpeed = 5f;
    public float baseTurnSpeed = 2f;
    public float canMoveRotationThreshold = 0.1f;
    
    [HideInInspector] public float movementSpeed;
    [HideInInspector] public float turnSpeed;
    private Rigidbody rb;
    private Animator anim;
    private Vector3 movement;

    private CameraManager cameraManager;
    private PlayerController playerController;

    //[FMODUnity.EventRef]
    //public string boost;

    [Header("Special / Dash")]
    [Range(0f, StaminaBar.FILL_MAX)] public float specialAwakenessDecrease = 0.5f;
    public List<TrailRenderer> specialTrailRenderers;


    private void Start()
    {
        movementSpeed = baseMovementSpeed;
        turnSpeed = baseTurnSpeed;
        rb = Utils.GetRequiredComponent<Rigidbody>(this);
        anim = Utils.GetRequiredComponentInChildren<Animator>(this);

        cameraManager = CameraManager.Instance;
        playerController = GameManager.Instance.GetPlayerController();

        ToggleSpecialTrailRenderers(false);
    }

    private void Update()
    {
        if (!playerController.EnablePlayerInput) return;

        HandleSpecialInput();
    }

    public void ResetVelocity()
    {
        rb.velocity = Vector3.zero;
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

    private void FixedUpdate()
    {
        if (!playerController.EnablePlayerInput || !cameraManager.IsActiveCameraValid() ||
            !Utils.InputAxisInUse(Constants.INPUT_AXIS_HORIZONTAL) && !Utils.InputAxisInUse(Constants.INPUT_AXIS_VERTICAL))
        {
            SetIsWalking(false);
            return;
        }

        float horizontal = Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL);
        float vertical = Input.GetAxis(Constants.INPUT_AXIS_VERTICAL);
        
        Vector3 rotatedDirection = playerController.AlignDirectionWithCamera(new Vector3(horizontal, 0, vertical));

        SetIsMovement(rotatedDirection);
        SetIsWalking(true);

        HandleControl(movement);
    }

    private void SetIsWalking(bool isWalking)
    {
        anim.SetBool(Constants.ANIMATION_BIRDIE_ISWALKING, isWalking);
    }

    private void SetIsMovement(Vector3 newMovement)
    {
        movement = newMovement.normalized;  // always want to normalize
    }

    private void HandleControl(Vector3 movementDirection)
    {
        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, movementDirection, turnSpeed * Time.fixedDeltaTime, 0f);

        HandleRotation(desiredForward);

        // Only start moving once we are close enough to our desired final rotation (which we are smoothly rotating towards)
        if ((movementDirection - desiredForward).magnitude < canMoveRotationThreshold)
        {
            HandleMovement();
        }
    }

    private void HandleRotation(Vector3 desiredForward)
    {
        rb.MoveRotation(Quaternion.LookRotation(desiredForward));
    }

    private void HandleMovement()
    {
        rb.MovePosition(rb.position + movement * (movementSpeed * Time.fixedDeltaTime));
    }
}