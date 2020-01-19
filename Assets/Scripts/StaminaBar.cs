using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public float speed = 0.001f;

    // Events for others to subscribe to OnChange events
    public delegate void ChangedAction(float fillAmount);
    public event ChangedAction OnChange;

    [HideInInspector] 
    public const float STAMINA_MAX = 1f;
    private Image staminaBarImage;

    private void Awake() 
    {
        staminaBarImage = Utils.GetRequiredComponent<Image>(this);
        staminaBarImage.fillAmount = 1f;
    }

    public IEnumerator DecreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float decreaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Max(0f, staminaBarImage.fillAmount - decreaseBy);

        while (fillAmount <= staminaBarImage.fillAmount)
        {
            UpdateFillAmount(staminaBarImage.fillAmount - speed);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public IEnumerator IncreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float increaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Min(STAMINA_MAX, staminaBarImage.fillAmount + increaseBy);

        while (fillAmount >= staminaBarImage.fillAmount)
        {
            UpdateFillAmount(staminaBarImage.fillAmount + speed);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    void UpdateFillAmount(float newFill)
    {
        // Update the fill amount
        staminaBarImage.fillAmount = newFill;

        OnChange?.Invoke(newFill);
    }


    
}
