using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Image fillImage;
    public GameObject glowOutline;
    public GameObject lightningRoot;

    public float fillChangePerSecond = 0.1f;
    public float fadeSpeed = 2f;
    public float decreasePerSecond = 0.2f;
    public float waitUntilDecreaseTime = 2f;

    [HideInInspector] public bool lightningActive = false;

    private List<Image> allChildImages;
    private Animator glowAnimator;
    private float lastIncreaseTime;

    public delegate void FadingComplete();
    public event FadingComplete OnFadingComplete;

    // Events for others to subscribe to OnChange events
    public delegate void ChangedAction(float fillAmount);
    public event ChangedAction OnChange;

    [HideInInspector] public const float FILL_MAX = 1f;

    [HideInInspector] public bool overrideValue = false;
    [HideInInspector] public float overrideTo;

    private void Awake()
    {
        allChildImages = GetComponentsInChildren<Image>(true).ToList();
        glowAnimator = glowOutline.GetComponent<Animator>();
    }

    void Start()
    {
        fillImage.fillAmount = 0.0f;
    }

    private void FixedUpdate()
    {
        if (overrideValue)
        {
            UpdateFillAmount(overrideTo);
            return;
        }

        if (Time.time > lastIncreaseTime + waitUntilDecreaseTime)
        {
            StartCoroutine(DecreaseFillBy(decreasePerSecond * Time.fixedDeltaTime));
        }
    }

    public void InstantIncreaseAwakenessBy(float amount)
    {
        UpdateFillAmount(fillImage.fillAmount + amount);
    }

    public void SetAwakeness(float amount)
    {
        UpdateFillAmount(amount);
    }

    public void ResetAwakeness()
    {
        UpdateFillAmount(0f);
    }

    // No custom decrease function needed so it was no implemented (like below)
    public IEnumerator DecreaseFillBy(float percent)
    {
        float percentClamped = Mathf.Clamp(percent, 0f, FILL_MAX);
        float decreaseBy = percentClamped * FILL_MAX;

        float goalFillAmount = Mathf.Max(0f, fillImage.fillAmount - decreaseBy);
        float toDecrease = fillImage.fillAmount - goalFillAmount;

        return ChangeFillGeneral(toDecrease, -fillChangePerSecond);
    }

    // Default IncreaseFillBy function, using the public changePerSecond field
    public IEnumerator IncreaseFillBy(float percent)
    {
        return IncreaseFillBy(percent, fillChangePerSecond);
    }

    // Allowing custom speed (specifically) for minigames, due to large increases being common
    public IEnumerator IncreaseFillBy(float percent, float speed) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, FILL_MAX);
        float increaseBy = percentClamped * FILL_MAX;

        float goalFillAmount = Mathf.Min(FILL_MAX, fillImage.fillAmount + increaseBy);
        float toAdd = goalFillAmount - fillImage.fillAmount;

        lastIncreaseTime = Time.time;

        return ChangeFillGeneral(toAdd, speed);
    }

    // Shortens the code, and keeps the main Coroutine loop logic in one place for updating the fillAmount
    IEnumerator ChangeFillGeneral(float difference, float speed)
    {
        float currentSum = 0f;
        while (currentSum <= difference)
        {
            float change = speed * Time.fixedDeltaTime;
            UpdateFillAmount(fillImage.fillAmount + change);
            currentSum += Mathf.Abs(change);  // because the change can be negative
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }

    void UpdateFillAmount(float newFill)
    {
        // Update the fill amount
        fillImage.fillAmount = newFill;

        EnableLightning(Mathf.Approximately(fillImage.fillAmount, FILL_MAX));

        OnChange?.Invoke(newFill);
    }

    private void EnableLightning(bool enable)
    {
        lightningActive = enable;
        lightningRoot.SetActive(enable);
    }

    //Fading in and out
    public void FadeOut()
    {
        // full color --> invisible
        allChildImages.ForEach(image =>
        {
            StartCoroutine(FadeAlphaCoroutine(1f, 0f, image));
        });
    }

    public void FadeIn()
    {
        // invisible --> full color
        allChildImages.ForEach(image =>
        {
            StartCoroutine(FadeAlphaCoroutine(0f, 1f, image));
        });
    }

    private IEnumerator FadeAlphaCoroutine(float startAlpha, float endAlpha, Image image)
    {
        float currentAlpha = image.color.a;
        while (Mathf.Abs(currentAlpha - startAlpha) < Mathf.Abs(startAlpha - endAlpha))
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, currentAlpha);
            currentAlpha += fadeSpeed * Time.deltaTime * Mathf.Sign(endAlpha - startAlpha);
            yield return null;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
        OnFadingComplete?.Invoke();

        yield return null;
    }

    private IEnumerator PulseColorCoroutine(Color startColor, Color endColor, Image image, int times = 4)
    {
        for (int i = 0; i < times; i++)
        {
            float currentLerp = 0f;
            while (currentLerp < 1f)
            {
                image.color = Color.Lerp(startColor, endColor, currentLerp);
                currentLerp += fadeSpeed * Time.deltaTime;
                yield return null;
            }
            image.color = Color.Lerp(startColor, endColor, 1f);

            currentLerp = 1f;
            while (currentLerp > 0f)
            {
                image.color = Color.Lerp(startColor, endColor, currentLerp);
                currentLerp -= fadeSpeed * Time.deltaTime;
                yield return null;
            }
            image.color = Color.Lerp(startColor, endColor, 0f);
        }
    }

    public void OuterGlow()
    {
        glowAnimator.SetTrigger("Glow");
    }

    public void InnerGlow(int times = 4)
    {
        StartCoroutine(PulseColorCoroutine(fillImage.color, Color.white, fillImage, times));
    }
}
