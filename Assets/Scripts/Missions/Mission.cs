using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMission : MonoBehaviour
{
    public delegate void MissionComplete();
    public event MissionComplete OnMissionComplete;

    public delegate void MissionReset();
    public event MissionReset OnMissionReset;

    protected void AlertMissionComplete()
    {
        OnMissionComplete?.Invoke();
    }

    protected void AlertMissionReset()
    {
        OnMissionReset?.Invoke();
    }
}
