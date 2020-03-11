using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODAwakeness : MonoBehaviour
{
    [FMODUnity.ParamRef]
    public string parameter;
    [FMODUnity.EventRef]
    public string sfx;
    private bool isFull = false;

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.staminaBar.OnChange += UpdateFMODAudio;
    }

    private void UpdateFMODAudio(float fillAmount)
    {
     FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameter, fillAmount);
        {
          if (fillAmount == 1f && !isFull)
            {
                isFull = true;
                FMODUnity.RuntimeManager.PlayOneShot(sfx, transform.position);
            }
        }
       if (fillAmount != 1f && isFull)
       { 
            isFull = false;
       }
    }

    
}
