using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionCheats: MonoBehaviour
{
    ////////////////////
    // TUTORIAL LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSION_TUTORIAL, true)]
    public static bool ValidateStartMissionTutorial()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_STARTMISSION_TUTORIAL)]
    public static void StartMissionTutorial()
    {
        MissionManager.Instance.StartMission(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_TUTORIAL, true)]
    public static bool ValidateStopMissionTutorial()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.MissionTutorial);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_TUTORIAL)]
    public static void EndMissionTutorial()
    {
        MissionManager.Instance.EndMission(MissionsEnum.MissionTutorial);
    }

    ////////////////////
    // KITCHEN LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSION_KITCHEN1, true)]
    public static bool ValidateStartMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_STARTMISSION_KITCHEN1)]
    public static void StartMissionCafeteria1()
    {
        MissionManager.Instance.StartMission(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_KITCHEN1, true)]
    public static bool ValidateStopMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.KitchenMission);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_KITCHEN1)]
    public static void EndMissionCafeteria1()
    {
        MissionManager.Instance.EndMission(MissionsEnum.KitchenMission);
    }

    ////////////////////
    // BRUTUS OFFICE SNEAK LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSION_BRUTUSOFFICESNEAK, true)]
    public static bool ValidateStartMissionBrutusOfficeSneak()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_STARTMISSION_BRUTUSOFFICESNEAK)]
    public static void StartMissionBrutusOfficeSneak()
    {
        MissionManager.Instance.StartMission(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_BRUTUSOFFICESNEAK, true)]
    public static bool ValidateStopMissionBrutusOfficeSneak()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.BrutusOfficeSneak);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_BRUTUSOFFICESNEAK)]
    public static void StopMissionBrutusOfficeSneak()
    {
        MissionManager.Instance.EndMission(MissionsEnum.BrutusOfficeSneak);
    }

    ////////////////////
    // HEDGE MAZE LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSION_HEDGEMAZE, true)]
    public static bool ValidateStartMissionHedgeMaze()
    {
        return Application.isPlaying && MissionManager.Instance && !MissionManager.Instance.IsMissionActive(MissionsEnum.HedgeMaze);
    }

    [MenuItem(Constants.CHEATS_STARTMISSION_HEDGEMAZE)]
    public static void StartMissionHedgeMaze()
    {
        MissionManager.Instance.StartMission(MissionsEnum.HedgeMaze);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_HEDGEMAZDE, true)]
    public static bool ValidateStopMissionHedgeMaze()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.IsMissionActive(MissionsEnum.HedgeMaze);
    }

    [MenuItem(Constants.CHEATS_ENDMISSION_HEDGEMAZDE)]
    public static void StopMissionHedgeMaze()
    {
        MissionManager.Instance.EndMission(MissionsEnum.HedgeMaze);
    }
}
