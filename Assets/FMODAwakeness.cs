using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODAwakeness : MonoBehaviour
{
    [FMODUnity.ParamRef]
    public string parameter;

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.staminaBar.OnChange += UpdateFMODAudio;
    }

    private void UpdateFMODAudio(float fillAmount)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameter, fillAmount);
    }
}
