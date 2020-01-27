using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AwakenessManager : Singleton<AwakenessManager>
{
    [Header("Post Processing")]
    public PostProcessVolume globalVolume;
    public float startShutterAngle = 1f;
    public float endShutterAngle = 360f;
    public float vignetteMultiplier = 66f;
    public float saturationMultiplier = -75f;
    public float contrastMultiplier = 75f;
    public float brightnessMultiplier = -25f;

    [Header("Movement Buffs")]
    public PlayerController playerController;
    public float movementSpeedBuff = 5f;
    public float turnSpeedBuff = 5f;
    
    private Vignette vignette;
    private MotionBlur motionBlur;
    private Grain filmGrain;
    private ColorGrading colorGrading;

    private void Start()
    {
        globalVolume.profile.TryGetSettings(out vignette);
        globalVolume.profile.TryGetSettings(out motionBlur);
        globalVolume.profile.TryGetSettings(out filmGrain);
        globalVolume.profile.TryGetSettings(out colorGrading);
        
        UIManager.Instance.staminaBar.OnChange += UpdateVignette;
        UIManager.Instance.staminaBar.OnChange += UpdateMotionBlur;
        UIManager.Instance.staminaBar.OnChange += UpdateFilmGrain;
        UIManager.Instance.staminaBar.OnChange += UpdateColorGrading;
        
        UIManager.Instance.staminaBar.OnChange += UpdateMovementBuffs;
    }

    void UpdateVignette(float fillAmount)
    {
        vignette.intensity.value = (vignetteMultiplier / 100f) * fillAmount;
    }
    
    void UpdateMotionBlur(float fillAmount)
    {
        // We have end --> start because: fillAmount == 0 means no stamina (end state), and == 1 means full (start state)
        motionBlur.shutterAngle.value = Mathf.Lerp(endShutterAngle, startShutterAngle, fillAmount / StaminaBar.STAMINA_MAX);
    }

    void UpdateFilmGrain(float fillAmount)
    {
        filmGrain.intensity.value = fillAmount;
    }
    
    void UpdateColorGrading(float fillAmount)
    {
        colorGrading.saturation.value = saturationMultiplier * fillAmount;
        colorGrading.contrast.value = contrastMultiplier * fillAmount;
        colorGrading.brightness.value = brightnessMultiplier * fillAmount;
    }

    void UpdateMovementBuffs(float fillAmount)
    {
        playerController.movementSpeed = playerController.baseMovementSpeed + (fillAmount * movementSpeedBuff);
        playerController.turnSpeed = playerController.baseTurnSpeed + (fillAmount * turnSpeedBuff);
    }
}
