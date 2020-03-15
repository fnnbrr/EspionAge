using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODMissionComplete : MonoBehaviour
{
    [FMODUnity.ParamRef]
    public string param;
    private bool completed;
    private bool SeekSpeed = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" == !completed)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(param, 1f, SeekSpeed);
            completed = true;
            Debug.Log("param 1f");
            ifCompleted();
        }
    }

    void ifCompleted ()
    {
        if (completed)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(param, 0f);
            completed = false;
            Debug.Log("param 0f");
        }
    }
}
