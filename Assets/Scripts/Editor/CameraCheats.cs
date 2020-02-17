using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraCheats : MonoBehaviour
{
    [MenuItem(Constants.CHEATS_CAMERA_BLENDTOFAR, true)]
    public static bool ValidateBlendToFar()
    {
        return Application.isPlaying && CameraManager.Instance;
    }

    [MenuItem(Constants.CHEATS_CAMERA_BLENDTOFAR)]
    public static void BlendToFar()
    {
        CameraManager.Instance.BlendToFarCameraForSeconds(1f);
    }
}
