using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AwakenessManager : Singleton<AwakenessManager>
{
    [Header("Awakeness Calculation")]
    public float awakenessIncrease = 0.01f;
    public float dangerRadius = 50.0f;
    
    [Header("Post Processing")]
    public Volume globalVolume;
    public float vignetteMultiplier = 0.45f;
    public float saturationMultiplier = -100f;
    public float contrastMultiplier = 10f;
    public float brightnessMultiplier = -0.5f;
    public float chromaticAberrationMultiplier = 0.25f;
    public float filmGrainMultiplier = 0.5f;

    [Header("Movement Buffs")]
    public float movementSpeedBuff = 5f;
    public float turnSpeedBuff = 5f;
    private MovementController movementController;

    [Header("Occlusion Effect")]
    public Material occlusionMaterial;

    [Header("Other Effects")]
    public float zoomInAmount;
    public Color emptyColor = Color.red;
    public Color fullColor = Color.yellow;

    private Vignette vignette;
    private MotionBlur motionBlur;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;

    private float startExposure;
    private float startContrast;

    private void Start()
    {   
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out motionBlur);
        globalVolume.profile.TryGet(out filmGrain);
        globalVolume.profile.TryGet(out colorAdjustments);
        globalVolume.profile.TryGet(out chromaticAberration);

        startExposure = colorAdjustments.postExposure.value;
        startContrast = colorAdjustments.contrast.value;

        UIManager.Instance.staminaBar.OnChange += UpdateVignette;
        //UIManager.Instance.staminaBar.OnChange += UpdateMotionBlur;
        UIManager.Instance.staminaBar.OnChange += UpdateFilmGrain;
        UIManager.Instance.staminaBar.OnChange += UpdateChromaticAberration;
        UIManager.Instance.staminaBar.OnChange += UpdateColorGrading;
        UIManager.Instance.staminaBar.OnChange += UpdateCameraDistance;
        UIManager.Instance.staminaBar.OnChange += UpdatePlayerAnimation;
        UIManager.Instance.staminaBar.OnChange += UpdateMovementBuffs;
        UIManager.Instance.staminaBar.OnChange += UpdateOcclusionColor;
        UIManager.Instance.staminaBar.OnChange += UpdateAwakenessBarFillColor;

        movementController = GameManager.Instance.GetMovementController();
    }

    private void FixedUpdate()
    {
        float minDistance = DistToClosestEnemy();

        if (minDistance < dangerRadius)
        {
            float awakenessGain = Mathf.Pow(((dangerRadius - minDistance) / dangerRadius), 2.0f);
            HandleIncreaseAwakeness(awakenessGain);
        }
    }
    
    private float DistToClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject enemy in enemies)
        {
            float curDistance = Vector3.Distance(enemy.transform.position, position);
            if (curDistance < minDistance)
            {
                minDistance = curDistance;
            }
        }
        return minDistance;
    }
    
    void HandleIncreaseAwakeness(float multiplier)
    {
        StartCoroutine(UIManager.Instance.staminaBar.IncreaseFillBy(multiplier * awakenessIncrease));
    }

    private void OnDisable()
    {
        // because this affect the actual asset, we reset the color back to the start color every time we end the game
        occlusionMaterial.SetColor("_BaseColor", emptyColor);
    }


    // // //  AWAKENESS LISTENERS // // //

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
        filmGrain.intensity.value = filmGrainMultiplier * GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateColorGrading(float fillAmount)
    {
        colorAdjustments.saturation.value = saturationMultiplier * GetInterpolatedFillAmount(fillAmount);
        colorAdjustments.contrast.value = startContrast + contrastMultiplier * GetInterpolatedFillAmount(fillAmount);
        colorAdjustments.postExposure.value = startExposure + brightnessMultiplier * GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateMovementBuffs(float fillAmount)
    {
        movementController.movementSpeed = movementController.baseMovementSpeed + (movementSpeedBuff * GetInterpolatedFillAmount(fillAmount));
        movementController.turnSpeed = movementController.baseTurnSpeed + (turnSpeedBuff * GetInterpolatedFillAmount(fillAmount));
    }

    private void UpdateOcclusionColor(float fillAmount)
    {
        occlusionMaterial.SetColor("_BaseColor", Color.Lerp(emptyColor, fullColor, GetInterpolatedFillAmount(fillAmount)));
    }

    private void UpdateAwakenessBarFillColor(float fillAmount)
    {
        float currentAlpha = UIManager.Instance.staminaBar.fillImage.color.a;
        Color lerpedColor = Color.Lerp(emptyColor, fullColor, GetInterpolatedFillAmount(fillAmount));
        UIManager.Instance.staminaBar.fillImage.color = new Color(lerpedColor.r, lerpedColor.g, lerpedColor.b, currentAlpha);
    }

    private void UpdateChromaticAberration(float fillAmount)
    {
        chromaticAberration.intensity.value = chromaticAberrationMultiplier * GetInterpolatedFillAmount(fillAmount);
    }

    private void UpdateCameraDistance(float fillAmount)
    {
        CameraManager.Instance.UpdateCameraDistances(zoomInAmount * GetInterpolatedFillAmount(fillAmount));
    }
}
