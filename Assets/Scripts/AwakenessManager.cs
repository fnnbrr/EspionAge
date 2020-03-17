using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AwakenessManager : Singleton<AwakenessManager>
{
    [Header("Awakeness Calculation")]
    public float awakenessIncrease = 0.01f;
    public float dangerRadius = 50.0f;
    
    private List<Coroutine> spawnedCoroutines;
    
    [Header("Post Processing")]
    public Volume globalVolume;
    public float vignetteMultiplier = 0.45f;
    public float saturationMultiplier = -100f;
    public float contrastMultiplier = 10f;
    public float brightnessMultiplier = -0.5f;

    [Header("Movement Buffs")]
    public float movementSpeedBuff = 5f;
    public float turnSpeedBuff = 5f;
    private MovementController movementController;

    [Header("Occlusion Effect")]
    public Material occlusionMaterial;
    public Color startOcclusionColor = Color.red;
    public Color endOcclusionColor = Color.yellow;

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
        spawnedCoroutines = new List<Coroutine>();
        
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out motionBlur);
        globalVolume.profile.TryGet(out filmGrain);
        globalVolume.profile.TryGet(out colorAdjustments);

        startExposure = colorAdjustments.postExposure.value;
        startContrast = colorAdjustments.contrast.value;

        UIManager.Instance.staminaBar.OnChange += UpdateVignette;
        //UIManager.Instance.staminaBar.OnChange += UpdateMotionBlur;
        UIManager.Instance.staminaBar.OnChange += UpdateFilmGrain;
        UIManager.Instance.staminaBar.OnChange += UpdateColorGrading;
        UIManager.Instance.staminaBar.OnChange += UpdateCameraDistance;
        UIManager.Instance.staminaBar.OnChange += UpdatePlayerAnimation;
        UIManager.Instance.staminaBar.OnChange += UpdateMovementBuffs;
        UIManager.Instance.staminaBar.OnChange += UpdateOcclusionColor;

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
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.IncreaseFillBy(multiplier * awakenessIncrease)));
    }
    
    // Note: Might need to do more testing if this is actually doing anything considerable...
    //  but better safe than sorry to make sure coroutines we spawn are no longer running when we enter a minigame
    void StopAllSpawnedCoroutines()
    {
        // No loose coroutines in MY house!
        foreach (Coroutine c in spawnedCoroutines)
        {
            StopCoroutine(c);
        }
        spawnedCoroutines.Clear();
    }

    private void OnDisable()
    {
        StopAllSpawnedCoroutines();

        occlusionMaterial.SetColor("_BaseColor", startOcclusionColor);
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
        movementController.movementSpeed = movementController.baseMovementSpeed + (movementSpeedBuff * GetInterpolatedFillAmount(fillAmount));
        movementController.turnSpeed = movementController.baseTurnSpeed + (turnSpeedBuff * GetInterpolatedFillAmount(fillAmount));
    }

    private void UpdateOcclusionColor(float fillAmount)
    {
        occlusionMaterial.SetColor("_BaseColor", Color.Lerp(startOcclusionColor, endOcclusionColor, GetInterpolatedFillAmount(fillAmount)));
    }

    private void UpdateCameraDistance(float fillAmount)
    {
        CameraManager.Instance.AddToCurrentCameraDistance(zoomInAmount * GetInterpolatedFillAmount(fillAmount));
    }
}
