using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Cheats: MonoBehaviour
{
    [MenuItem(Constants.CHEATS_ASSET_PATH_STARTMISSIONCAFETERIA1, true)]
    public static bool ValidateStartMissionCafeteria1()
    {
        return MissionManager.Instance.GetActiveMission<MissionCafeteria1>() == null;
    }

    [MenuItem(Constants.CHEATS_ASSET_PATH_STARTMISSIONCAFETERIA1)]
    public static void StartMissionCafeteria1()
    {
        if (Application.isPlaying)
        {
            if (!MissionManager.Instance)
            {
                Debug.LogError("There is not MissionManager instance in the scene!");
                return;
            }

            // Possible issue, if we change the path this asset is at, we gotta update this path
            MissionManager.Instance.StartMission(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Missions/MissionCafeteria1.prefab"));
        }
        else
        {
            Debug.LogError("Not in play mode.");
        }
    }

    [MenuItem(Constants.CHEATS_ASSET_PATH_ENDMISSIONCAFETERIA1, true)]
    public static bool ValidateStopMissionCafeteria1()
    {
        return MissionManager.Instance.GetActiveMission<MissionCafeteria1>() != null;
    }

    [MenuItem(Constants.CHEATS_ASSET_PATH_ENDMISSIONCAFETERIA1)]
    public static void EndMissionCafeteria1()
    {
        if (Application.isPlaying)
        {
            if (!MissionManager.Instance)
            {
                Debug.LogError("There is not MissionManager instance in the scene!");
                return;
            }

            IMission mission = MissionManager.Instance.GetActiveMission<MissionCafeteria1>().mission;
            MissionManager.Instance.EndMission(mission);
        }
        else
        {
            Debug.LogError("Not in play mode.");
        }
    }
}
