using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Image staminaFillImage;
    public float changePerSecond = 0.1f;

    // Events for others to subscribe to OnChange events
    public delegate void ChangedAction(float fillAmount);
    public event ChangedAction OnChange;

    [HideInInspector] 
    public const float STAMINA_MAX = 1f;

    private void Awake() 
    {
        staminaFillImage.fillAmount = 0.0f;
    }

    // No custom decrease function needed so it was no implemented (like below)
    public IEnumerator DecreaseStaminaBy(float percent)
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float decreaseBy = percentClamped * STAMINA_MAX;

        float goalFillAmount = Mathf.Max(0f, staminaFillImage.fillAmount - decreaseBy);
        float toDecrease = staminaFillImage.fillAmount - goalFillAmount;

        return ChangeStaminaGeneral(toDecrease, -changePerSecond);
    }

    // Default IncreaseStaminaBy function, using the public changePerSecond field
    public IEnumerator IncreaseStaminaBy(float percent)
    {
        return IncreaseStaminaBy(percent, changePerSecond);
    }

    // Allowing custom speed (specifically) for minigames, due to large increases being common
    public IEnumerator IncreaseStaminaBy(float percent, float speed) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float increaseBy = percentClamped * STAMINA_MAX;

        float goalFillAmount = Mathf.Min(STAMINA_MAX, staminaFillImage.fillAmount + increaseBy);
        float toAdd = goalFillAmount - staminaFillImage.fillAmount;

        return ChangeStaminaGeneral(toAdd, speed);
    }

    // Shortens the code, and keeps the main Coroutine loop logic in one place for updating the fillAmount
    IEnumerator ChangeStaminaGeneral(float difference, float speed)
    {
        float currentSum = 0f;
        while (currentSum <= difference)
        {
            float change = speed * Time.fixedDeltaTime;
            UpdateFillAmount(staminaFillImage.fillAmount + change);
            currentSum += Mathf.Abs(change);  // because the change can be negative
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }

    void UpdateFillAmount(float newFill)
    {
        // Update the fill amount
        staminaFillImage.fillAmount = newFill;

        OnChange?.Invoke(newFill);
    }
}
