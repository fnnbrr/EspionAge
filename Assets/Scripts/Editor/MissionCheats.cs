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

    ////////////////////
    // BRUTUS OFFICE SNEAK LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSIONBRUTUSOFFICESNEAK, true)]
    public static bool ValidateStartMissionBrutusOfficeSneak()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONBRUTUSOFFICESNEAK)]
    public static void StartMissionBrutusOfficeSneak()
    {
        MissionManager.Instance.StartMission(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONBRUTUSOFFICESNEAK, true)]
    public static bool ValidateStopMissionBrutusOfficeSneak()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONBRUTUSOFFICESNEAK)]
    public static void StopMissionBrutusOfficeSneak()
    {
        MissionManager.Instance.EndMission(MissionsEnum.BrutusOfficeSneak);
    }
}
