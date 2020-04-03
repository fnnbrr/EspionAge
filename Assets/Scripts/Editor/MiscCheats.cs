using UnityEngine;
using UnityEditor;

public class MiscCheats : MonoBehaviour
{
    public static bool Validate()
    {
        return Application.isPlaying;
    }

    public static void SetTimeScale(float amount)
    {
        Time.timeScale = amount;
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_SLOWMOTION, true)]
    public static bool ValidateSetTimeScaleSlow()
    {
        return Validate();
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_SLOWMOTION)]
    public static void SetTimeScaleSlow()
    {
        SetTimeScale(0.5f);
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_REGULAR, true)]
    public static bool ValidateSetTimeScaleRegular()
    {
        return Validate();
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_REGULAR)]
    public static void SetTimeScaleRegular()
    {
        SetTimeScale(1f);
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_DOUBLE, true)]
    public static bool ValidateSetTimeScaleDouble()
    {
        return Validate();
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_DOUBLE)]
    public static void SetTimeScaleDouble()
    {
        SetTimeScale(2f);
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_TRIPLE, true)]
    public static bool ValidateSetTimeScaleTriple()
    {
        return Validate();
    }

    [MenuItem(Constants.CHEATS_MISC_TIME_TRIPLE)]
    public static void SetTimeScaleTriple()
    {
        SetTimeScale(3f);
    }
}
