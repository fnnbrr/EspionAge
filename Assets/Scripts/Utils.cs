using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static T GetRequiredComponent<T>(MonoBehaviour o) where T : Component
    {
        T comp = o.GetComponent<T>();
        if (!comp)
        {
            Debug.LogError($"Expected component '{typeof(T).Name}' on object {o.name}");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        return comp;
    }
}
