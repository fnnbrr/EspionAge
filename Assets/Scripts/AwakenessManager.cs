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

    [Header("Other Effects")]
    public float zoomInAmount;

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
        UIManager.Instance.staminaBar.OnChange += UpdateCameraDistance;
        UIManager.Instance.staminaBar.OnChange += UpdatePlayerAnimation;
        
        UIManager.Instance.staminaBar.OnChange += UpdateMovementBuffs;
    }

    private void UpdatePlayerAnimation(float fillAmount)
    {
        GameManager.Instance.GetPlayerAnimator().SetFloat(Constants.ANIMATION_BIRDIE_AWAKENESS, fillAmount);
    }

    private float GetInterpolatedFillAmount(float fillAmount)
    {
        return fillAmount / StaminaBar.FILL_MAX;  // this is just dividing by 1 for now, but this could cause many issues if we ever change it
    }

    private void UpdateVignette(float fillAmount)
    {
        vignette.intensity.value = vignetteMultiplier * GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateMotionBlur(float fillAmount)
    {
        // We have end --> start because: fillAmount == 0 means no stamina (end state), and == 1 means full (start state)
        motionBlur.intensity.value = GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateFilmGrain(float fillAmount)
    {
        filmGrain.intensity.value = GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateColorGrading(float fillAmount)
    {
        colorAdjustments.saturation.value = saturationMultiplier * GetInterpolatedFillAmount(fillAmount);
        colorAdjustments.contrast.value = startContrast + contrastMultiplier * GetInterpolatedFillAmount(fillAmount);
        colorAdjustments.postExposure.value = startExposure + brightnessMultiplier * GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateMovementBuffs(float fillAmount)
    {
        playerController.movementSpeed = playerController.baseMovementSpeed + (movementSpeedBuff * GetInterpolatedFillAmount(fillAmount));
        playerController.turnSpeed = playerController.baseTurnSpeed + (turnSpeedBuff * GetInterpolatedFillAmount(fillAmount));
    }

    private void UpdateCameraDistance(float fillAmount)
    {
        CameraManager.Instance.AddToCurrentCameraDistance(zoomInAmount * GetInterpolatedFillAmount(fillAmount));
    }
}
