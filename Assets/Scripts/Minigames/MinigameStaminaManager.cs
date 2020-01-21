using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameStaminaManager : MonoBehaviour
{
    public MinigameBarSetup minigameBarSetup;
    public Image barImage;

    private const float MAX_FILL = 1f;

    public enum TimeModeType
    {
        SpecifyStaminaDecrease,
        SpecifyStaminaIncrease,
        SpecifyMaxTime
    }
    [Header("Time Mode Options")]
    public TimeModeType timeModeType;
    [Range(0f, 1f)] public float staminaChangeAmountPerSecond;
    public float maxTime;

    // For whenever we add options for the target mode
    //[Header("Target Mode Options")]

    private void Start()
    {
        switch (timeModeType)
        {
            case TimeModeType.SpecifyStaminaDecrease:
                SetStamina(MAX_FILL);
                break;
            case TimeModeType.SpecifyStaminaIncrease:
                SetStamina(0f);
                break;
            case TimeModeType.SpecifyMaxTime:
                SetStamina(MAX_FILL);
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        switch(timeModeType)
        {
            case TimeModeType.SpecifyStaminaDecrease:
                IncreaseStaminaBy(-staminaChangeAmountPerSecond * Time.fixedDeltaTime);
                break;
            case TimeModeType.SpecifyStaminaIncrease:
                IncreaseStaminaBy(staminaChangeAmountPerSecond * Time.fixedDeltaTime);
                break;
            case TimeModeType.SpecifyMaxTime:
                IncreaseStaminaBy(-MAX_FILL / maxTime * Time.fixedDeltaTime);
                break;
            default:
                break;
        }
    }

    void SetStamina(float setTo)
    {
        barImage.fillAmount = setTo;
    }

    void IncreaseStaminaBy(float increaseBy)
    {
        barImage.fillAmount += increaseBy;
    }
}
