using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSFX : MonoBehaviour
{
    void Step(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }
}
