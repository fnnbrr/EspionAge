using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionCheats: MonoBehaviour
{
    ////////////////////
    // TUTORIAL LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSIONTUTORIAL, true)]
    public static bool ValidateStartMissionTutorial()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONTUTORIAL)]
    public static void StartMissionTutorial()
    {
        MissionManager.Instance.StartMission(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONTUTORIAL, true)]
    public static bool ValidateStopMissionTutorial()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONTUTORIAL)]
    public static void EndMissionTutorial()
    {
        MissionManager.Instance.EndMission(MissionsEnum.MissionTutorial);
    }

    ////////////////////
    // KITCHEN LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSIONKITCHEN1, true)]
    public static bool ValidateStartMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONKITCHEN1)]
    public static void StartMissionCafeteria1()
    {
        MissionManager.Instance.StartMission(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONKITCHEN1, true)]
    public static bool ValidateStopMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONKITCHEN1)]
    public static void EndMissionCafeteria1()
    {
        MissionManager.Instance.EndMission(MissionsEnum.KitchenMission);
    }
}
