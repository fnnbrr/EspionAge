using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Image staminaFillImage;
    public float speed = 0.001f;

    // Events for others to subscribe to OnChange events
    public delegate void ChangedAction(float fillAmount);
    public event ChangedAction OnChange;

    [HideInInspector] 
    public const float STAMINA_MAX = 1f;

    private void Awake() 
    {
        staminaFillImage.fillAmount = 1f;
    }

    public IEnumerator DecreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float decreaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Max(0f, staminaFillImage.fillAmount - decreaseBy);

        while (fillAmount <= staminaFillImage.fillAmount)
        {
            UpdateFillAmount(staminaFillImage.fillAmount - speed);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public IEnumerator IncreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float increaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Min(STAMINA_MAX, staminaFillImage.fillAmount + increaseBy);

        while (fillAmount >= staminaFillImage.fillAmount)
        {
            UpdateFillAmount(staminaFillImage.fillAmount + speed);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    void UpdateFillAmount(float newFill)
    {
        // Update the fill amount
        staminaFillImage.fillAmount = newFill;

        OnChange?.Invoke(newFill);
    }
}
