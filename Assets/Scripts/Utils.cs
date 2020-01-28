using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void StopPlayMode()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public static void LogErrorAndStopPlayMode(string errorMessage)
    {
        Debug.LogError(errorMessage);
        StopPlayMode();
    }

    public static T GetRequiredComponent<T>(MonoBehaviour o) where T : Component
    {
        return GetRequiredComponent<T>(o, $"Expected component '{typeof(T).Name}' on object {o.name}");
    }

    public static T GetRequiredComponent<T>(GameObject o) where T : Component
    {
        return GetRequiredComponent<T>(o, $"Expected component '{typeof(T).Name}' on object {o.name}");
    }

    public static T GetRequiredComponent<T>(MonoBehaviour o, string customError) where T : Component
    {
        T comp = o.GetComponent<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static T GetRequiredComponent<T>(GameObject o, string customError) where T : Component
    {
        T comp = o.GetComponent<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static int PingPong(int t, int length)
    {
        int q = t / length;
        int r = t % length;

        if ((q % 2) == 0)
        {
            return r;
        }
        else
        {
            return length - r;
        }
    }
}
