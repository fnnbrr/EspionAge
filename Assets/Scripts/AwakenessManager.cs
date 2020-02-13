using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AwakenessManager : Singleton<AwakenessManager>
{
    [Header("Post Processing")]
    public Volume globalVolume;
    public float vignetteMultiplier = 0.45f;
    public float saturationMultiplier = -100f;
    public float contrastMultiplier = 10f;
    public float brightnessMultiplier = -0.5f;

    [Header("Movement Buffs")]
    public PlayerController playerController;
    public float movementSpeedBuff = 5f;
    public float turnSpeedBuff = 5f;

    private Vignette vignette;
    private MotionBlur motionBlur;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;

    private float startExposure;
    private float startContrast;

    private void Start()
    {
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out motionBlur);
        globalVolume.profile.TryGet(out filmGrain);
        globalVolume.profile.TryGet(out colorAdjustments);

        startExposure = colorAdjustments.postExposure.value;
        startContrast = colorAdjustments.contrast.value;

        UIManager.Instance.staminaBar.OnChange += UpdateVignette;
        UIManager.Instance.staminaBar.OnChange += UpdateMotionBlur;
        UIManager.Instance.staminaBar.OnChange += UpdateFilmGrain;
        UIManager.Instance.staminaBar.OnChange += UpdateColorGrading;
        
        UIManager.Instance.staminaBar.OnChange += UpdateMovementBuffs;
    }

    void UpdateVignette(float fillAmount)
    {
        vignette.intensity.value = vignetteMultiplier * fillAmount;
    }
    
    void UpdateMotionBlur(float fillAmount)
    {
        // We have end --> start because: fillAmount == 0 means no stamina (end state), and == 1 means full (start state)
        motionBlur.intensity.value = fillAmount / StaminaBar.STAMINA_MAX;
    }

    void UpdateFilmGrain(float fillAmount)
    {
        filmGrain.intensity.value = fillAmount / StaminaBar.STAMINA_MAX;
    }
    
    void UpdateColorGrading(float fillAmount)
    {
        colorAdjustments.saturation.value = saturationMultiplier * fillAmount;
        colorAdjustments.contrast.value = startContrast + contrastMultiplier * fillAmount;
        colorAdjustments.postExposure.value = startExposure + brightnessMultiplier * fillAmount;
    }

    void UpdateMovementBuffs(float fillAmount)
    {
        playerController.movementSpeed = playerController.baseMovementSpeed + (fillAmount * movementSpeedBuff);
        playerController.turnSpeed = playerController.baseTurnSpeed + (fillAmount * turnSpeedBuff);
    }
}
