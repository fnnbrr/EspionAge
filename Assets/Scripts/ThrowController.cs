using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("Throwing")]
    [Range(1.0f, 10.0f)] public float sensitivityMouse = 2.0f;
    [Range(1.0f, 10.0f)] public float sensitivityController = 2.0f;
    public Transform throwPosition;
    public float throwableDestroyTime = 5f;
    public float throwMultiplier = 0.08f;
    public float minThrowVelocity = 10f;
    public float maxThrowVelocity = 20f;

    private LaunchArcRenderer launchArcRenderer;
    private List<GameObject> currentThrowables;
    private Plane mouseHitPlane;
    private Camera mainCamera;
    private bool mainCameraActive = false;
    private bool isThrowReset = true;

    public delegate void OnThrowEventHandler(Interactable source);
    public event OnThrowEventHandler OnThrow;

    public delegate void OnPickupEventHandler(GameObject source);
    public event OnPickupEventHandler OnPickup;
    
    public delegate void OnThrowableResetEventHandler();
    public event OnThrowableResetEventHandler OnThrowableReset;

    public delegate void OnInteractEventHandler(DialogueInteractable source);
    public event OnInteractEventHandler OnInteractBegin;

    private void Start()
    {
        launchArcRenderer = GetComponentInChildren<LaunchArcRenderer>();
        launchArcRenderer.gameObject.SetActive(false);  // initially hide arc
        currentThrowables = new List<GameObject>();
        
        mouseHitPlane = new Plane(Vector3.up, Vector3.zero);
        mainCamera = Camera.main;

        UIManager.Instance.staminaBar.OnChange += UpdateThrowVelocity;
    }

    private void Update()
    {
        HandleThrowInput();
    }

    private void UpdateThrowVelocity(float fillAmount)
    {
        launchArcRenderer.velocity = Mathf.Lerp(minThrowVelocity, maxThrowVelocity, fillAmount / StaminaBar.FILL_MAX);
    }

    public Vector3 GetMousePosition()
    {
        if (mainCameraActive)
        {
            Vector3 position = transform.position;
            
            // Handle controller input
            if (GameManager.Instance.GetPlayerController().controllerConnected && 
                (Utils.InputAxisInUse(Constants.INPUT_AXIS_HORIZONTAL_RIGHT_STICK) || 
                 Utils.InputAxisInUse(Constants.INPUT_AXIS_VERTICAL_RIGHT_STICK)))
            {
                float horizontal = 10 * sensitivityController * Input.GetAxis(Constants.INPUT_AXIS_VERTICAL_RIGHT_STICK);
                float vertical = 10 * sensitivityController * Input.GetAxis(Constants.INPUT_AXIS_HORIZONTAL_RIGHT_STICK);
                
                return new Vector3(position.x + horizontal, position.y, position.z + vertical);
            }
            
            // Handle mouse + keyboard input
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    
            if(mouseHitPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                
                float horizontal = sensitivityMouse * (hitPoint.x - position.x);
                float vertical = sensitivityMouse * (hitPoint.z - position.z);
            
                return new Vector3(position.x + horizontal, position.y, position.z + vertical);
            }
        }

        if (mainCamera != null) mainCameraActive = true;
        return transform.position;
    }

    private void HandleThrowInput()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput || currentThrowables.Count <= 0) return;

        // Handle controller input
        if (GameManager.Instance.GetPlayerController().controllerConnected)
        {
            bool isTriggerDown = Utils.InputAxisInUse(Constants.INPUT_THROW_GETDOWN);

            // Right joystick is being used
            if (Utils.InputAxisInUse(Constants.INPUT_AXIS_HORIZONTAL_RIGHT_STICK) || 
                Utils.InputAxisInUse(Constants.INPUT_AXIS_VERTICAL_RIGHT_STICK))
            {
                if (!launchArcRenderer.gameObject.activeInHierarchy)
                {
                    // Start rendering throw arc
                    launchArcRenderer.gameObject.SetActive(true);
                }
                else if (isTriggerDown && isThrowReset)
                {
                    isThrowReset = false;
                    ThrowNext();
                }
            }

            // Right joystick (and mouse) is not being used but arc is still being rendered
            else if (launchArcRenderer.gameObject.activeInHierarchy && 
                     !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0))
            {
                // Stop rendering throw arc
                launchArcRenderer.gameObject.SetActive(false);
            }

            // Reset trigger when released after being held down
            if (!isThrowReset && !isTriggerDown)
            {
                isThrowReset = true;
            }
        }

        // Handle mouse + keyboard input
        if (Input.GetMouseButtonDown(0) && !launchArcRenderer.gameObject.activeInHierarchy)
        {
            // Start rendering throw arc
            launchArcRenderer.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(0) && launchArcRenderer.gameObject.activeInHierarchy)
        {
            ThrowNext();
        }
        else if (Input.GetMouseButtonDown(1) && launchArcRenderer.gameObject.activeInHierarchy)
        {
            // Stop rendering throw arc
            launchArcRenderer.gameObject.SetActive(false);
        }
    }

    public void AddThrowable(GameObject throwableObject)
    {
        throwableObject.SetActive(false);
        throwableObject.transform.parent = throwPosition;
        throwableObject.transform.localPosition = Vector3.zero;

        currentThrowables.Add(throwableObject);

        OnPickup?.Invoke(throwableObject);
    }

    // This is an unsafe function! Must check length of currentThrowables before calling it (like in HandleThrowInput() above)!
    private void ThrowNext()
    {
        GameObject current = currentThrowables[0];
        currentThrowables.RemoveAt(0);

        Rigidbody currentRigidbody = current.GetComponent<Rigidbody>();
        if (currentRigidbody)
        {
            // Display the object, center at the throw point, remove parent, then throw in the same path of the current launch arc
            // - the force angle is partly from just testing and playing with the values
            current.SetActive(true);
            current.transform.localPosition = Vector3.zero;
            current.transform.parent = null;
            currentRigidbody.AddForce(Quaternion.AngleAxis((launchArcRenderer.angle % 180f) - 90, launchArcRenderer.transform.forward) * launchArcRenderer.transform.up * (launchArcRenderer.velocity * throwMultiplier), ForceMode.Impulse);
        }

        // Disable/hide the launchArcRenderer after throwing
        launchArcRenderer.gameObject.SetActive(false);

        OnThrow?.Invoke(current.GetComponent<Interactable>());

        StartCoroutine(FadeOutAndDelete(current, throwableDestroyTime));
    }

    public void ResetThrowables()
    {
        currentThrowables.ForEach(Destroy);
        currentThrowables.Clear();

        OnThrowableReset?.Invoke();
    }

    private IEnumerator FadeOutAndDelete(GameObject o, float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);

        ObjectFader objectFader = o.GetComponent<ObjectFader>();
        if (!objectFader)
        {
            Destroy(o);
        }
        else
        {
            objectFader.OnFadeToTransparentComplete += () => Destroy(o);
            objectFader.FadeToTransparent();
        }
    }
    
    public void InteractPlayer(DialogueInteractable source)
    {
        OnInteractBegin?.Invoke(source);
    }
}
