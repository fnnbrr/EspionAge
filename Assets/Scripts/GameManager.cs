using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : Singleton<GameManager>
{
    [Header("Post Processing")]
    public PostProcessVolume globalVolume;
    private Vignette vignette;

    private void Start()
    {
        globalVolume.profile.TryGetSettings(out vignette);
        UIManager.Instance.staminaBar.OnChange += UpdateVignette;
    }

    void UpdateVignette(float fillAmount)
    {
        vignette.intensity.value = StaminaBar.STAMINA_MAX - fillAmount;
    }
}
