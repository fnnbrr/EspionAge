using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AwakenessCheats : MonoBehaviour
{
    [MenuItem(Constants.CHEATS_AWAKENESS_DEFAULT, true)]
    public static bool ValidateAwakenessDefault()
    {
        return Application.isPlaying && UIManager.Instance && UIManager.Instance.staminaBar && UIManager.Instance.staminaBar.overrideValue;
    }

    [MenuItem(Constants.CHEATS_AWAKENESS_DEFAULT)]
    public static void AwakenessDefault()
    {
        UIManager.Instance.staminaBar.overrideValue = false;
    }

    [MenuItem(Constants.CHEATS_AWAKENESS_ALWAYSMIN, true)]
    public static bool ValidateAwakenessAlwaysMin()
    {
        return Application.isPlaying && UIManager.Instance && UIManager.Instance.staminaBar;
    }

    [MenuItem(Constants.CHEATS_AWAKENESS_ALWAYSMIN)]
    public static void AwakenessAlwaysMin()
    {
        UIManager.Instance.staminaBar.overrideValue = true;
        UIManager.Instance.staminaBar.overrideTo = 0f;
    }

    [MenuItem(Constants.CHEATS_AWAKENESS_ALWAYSMAX, true)]
    public static bool ValidateAwakenessAlwaysMax()
    {
        return Application.isPlaying && UIManager.Instance && UIManager.Instance.staminaBar;
    }

    [MenuItem(Constants.CHEATS_AWAKENESS_ALWAYSMAX)]
    public static void AwakenessAlwaysMax()
    {
        UIManager.Instance.staminaBar.overrideValue = true;
        UIManager.Instance.staminaBar.overrideTo = StaminaBar.FILL_MAX;
    }
}
