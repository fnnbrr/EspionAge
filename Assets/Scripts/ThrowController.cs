using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("Throwing")]
    public Transform throwPosition;
    public float throwableDestroyTime = 5f;
    public float throwMultiplier = 0.08f;
    public float minThrowVelocity = 10f;
    public float maxThrowVelocity = 20f;

    private LaunchArcRenderer launchArcRenderer;
    private List<GameObject> currentThrowables;

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

    private void HandleThrowInput()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput || currentThrowables.Count <= 0) return;

        // Handle mouse + keyboard input
        if (!GameManager.Instance.GetPlayerController().controllerConnected)
        {
            // Start rendering throw arc
            if (Input.GetMouseButtonDown(0) && !launchArcRenderer.gameObject.activeInHierarchy)
            {
                launchArcRenderer.gameObject.SetActive(true);
            }
            // Throw
            else if (Input.GetMouseButtonUp(0) && launchArcRenderer.gameObject.activeInHierarchy)
            {
                ThrowNext();
            }
            // Stop rendering throw arc
            else if (Input.GetMouseButtonDown(1) && launchArcRenderer.gameObject.activeInHierarchy)
            {
                launchArcRenderer.gameObject.SetActive(false);
            }
        }

        // Handle controller input
        else
        {
            float throwAxisValue = Input.GetAxis(Constants.INPUT_THROW_GETDOWN);
            
            // Trigger is held down
            if (!Mathf.Approximately(throwAxisValue, 0f) && launchArcRenderer.gameObject.activeInHierarchy)
            {
                ThrowNext();
            }
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
            currentRigidbody.AddForce(Quaternion.AngleAxis((launchArcRenderer.angle % 180f) - 90, launchArcRenderer.transform.forward) * launchArcRenderer.transform.up * launchArcRenderer.velocity * throwMultiplier, ForceMode.Impulse);
        }

        // Disable/hide the launchArcRenderer after throwing
        launchArcRenderer.gameObject.SetActive(false);

        OnThrow?.Invoke(current.GetComponent<Interactable>());

        StartCoroutine(FadeOutAndDelete(current, throwableDestroyTime));
    }

    public void ResetThrowables()
    {
        currentThrowables.ForEach(t =>
        {
            Destroy(t);
        });
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
