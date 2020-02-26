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
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionTutorial>() == null;
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONTUTORIAL)]
    public static void StartMissionTutorial()
    {
        MissionManager.Instance.StartMission(AssetDatabase.LoadAssetAtPath<GameObject>(Constants.ASSET_PATH_MISSIONTUTORIAL));
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONTUTORIAL, true)]
    public static bool ValidateStopMissionTutorial()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionTutorial>() != null;
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONTUTORIAL)]
    public static void EndMissionTutorial()
    {
        AMission mission = MissionManager.Instance.GetActiveMission<MissionTutorial>().mission;
        MissionManager.Instance.EndMission(mission);
    }

    ////////////////////
    // KITCHEN LEVEL
    ////////////////////
    [MenuItem(Constants.CHEATS_STARTMISSIONKITCHEN1, true)]
    public static bool ValidateStartMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionKitchen1>() == null;
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONKITCHEN1)]
    public static void StartMissionCafeteria1()
    {
        MissionManager.Instance.StartMission(AssetDatabase.LoadAssetAtPath<GameObject>(Constants.ASSET_PATH_MISSIONCAFETERIA1));
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONKITCHEN1, true)]
    public static bool ValidateStopMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionKitchen1>() != null;
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONKITCHEN1)]
    public static void EndMissionCafeteria1()
    {
        AMission mission = MissionManager.Instance.GetActiveMission<MissionKitchen1>().mission;
        MissionManager.Instance.EndMission(mission);
    }
}
