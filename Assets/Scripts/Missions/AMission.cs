using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMission : MonoBehaviour
{
    protected bool isInitialized = false;

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

    public void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }
    }

    private void OnDisable()
    {
        if (isInitialized)
        {
            Cleanup();
            isInitialized = false;
        }
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            Cleanup();
            isInitialized = false;
        }
    }

    protected virtual void Initialize()
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Cleanup()
    {
        throw new System.NotImplementedException();
    }
}
