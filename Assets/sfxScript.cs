using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sfxScript : MonoBehaviour
{
    // Update is called once per frame
    void LightningSFX(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
        Debug.Log("LightningSFX is triggered");
    }

    void Step(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
        Debug.Log("StepSFX is triggered");
    }

    void RollSFX(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
        Debug.Log("RollSFX is triggered");
    }
}
