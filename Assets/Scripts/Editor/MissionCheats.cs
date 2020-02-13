using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissionCheats: MonoBehaviour
{
    [MenuItem(Constants.CHEATS_STARTMISSIONCAFETERIA1, true)]
    public static bool ValidateStartMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionCafeteria1>() == null;
    }

    [MenuItem(Constants.CHEATS_STARTMISSIONCAFETERIA1)]
    public static void StartMissionCafeteria1()
    {
        MissionManager.Instance.StartMission(AssetDatabase.LoadAssetAtPath<GameObject>(Constants.ASSET_PATH_MISSIONCAFETERIA1));
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONCAFETERIA1, true)]
    public static bool ValidateStopMissionCafeteria1()
    {
        return Application.isPlaying && MissionManager.Instance && MissionManager.Instance.GetActiveMission<MissionCafeteria1>() != null;
    }

    [MenuItem(Constants.CHEATS_ENDMISSIONCAFETERIA1)]
    public static void EndMissionCafeteria1()
    {
        AMission mission = MissionManager.Instance.GetActiveMission<MissionCafeteria1>().mission;
        MissionManager.Instance.EndMission(mission);
    }
}
