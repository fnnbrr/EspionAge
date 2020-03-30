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

    public static T GetRequiredComponent<T>(GameObject o) where T : Component
    {
        return GetRequiredComponent<T>(o, $"Expected component '{typeof(T).Name}' on object {o.name}");
    }

    public static T GetRequiredComponent<T>(MonoBehaviour o) where T : Component
    {
        return GetRequiredComponent<T>(o, $"Expected component '{typeof(T).Name}' on object {o.name}");
    }

    public static T GetRequiredComponentInChildren<T>(GameObject o) where T : Component
    {
        return GetRequiredComponentInChildren<T>(o, $"Expected component '{typeof(T).Name}' in children of object {o.name}");
    }

    public static T GetRequiredComponentInChildren<T>(MonoBehaviour o) where T : Component
    {
        return GetRequiredComponentInChildren<T>(o, $"Expected component '{typeof(T).Name}' in children of object {o.name}");
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

    public static T GetRequiredComponent<T>(MonoBehaviour o, string customError) where T : Component
    {
        T comp = o.GetComponent<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static T GetRequiredComponentInChildren<T>(GameObject o, string customError) where T : Component
    {
        T comp = o.GetComponentInChildren<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static T GetRequiredComponentInChildren<T>(MonoBehaviour o, string customError) where T : Component
    {
        T comp = o.GetComponentInChildren<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static T GetRequiredComponentInParent<T>(MonoBehaviour o) where T : Component
    {
        return GetRequiredComponentInParent<T>(o, $"Expected component '{typeof(T).Name}' in parent of object {o.name}");
    }

    public static T GetRequiredComponentInParent<T>(MonoBehaviour o, string customError) where T : Component
    {
        T comp = o.GetComponentInParent<T>();
        if (!comp)
        {
            LogErrorAndStopPlayMode(customError);
        }
        return comp;
    }

    public static bool HasComponent<T>(this GameObject obj)
    {
        return obj.GetComponent(typeof(T)) != null;
    }

    public static bool HasComponentInParent<T>(this GameObject obj)
    {
        return obj.GetComponentInParent(typeof(T)) != null;
    }

    public static bool HasComponentInChildren<T>(this GameObject obj)
    {
        return obj.GetComponentInChildren(typeof(T)) != null;
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

    public static bool InputAxisInUse(string axisName)
    {
        return !Mathf.Approximately(Input.GetAxis(axisName), 0f);
    }
}
