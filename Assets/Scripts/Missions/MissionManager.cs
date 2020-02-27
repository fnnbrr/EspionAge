using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MissionObject
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation;

    [HideInInspector]
    public GameObject spawnedInstance;
}

public class InProgressMissionContainer
{
    public AMission mission;
    public GameObject gameObject;

    public InProgressMissionContainer(AMission _mission, GameObject _gameObject)
    {
        mission = _mission;
        gameObject = _gameObject;
    }
}

[System.Serializable]
public class MissionMapping
{
    [Header("Make sure this mission is unique among the list!")]
    public MissionsEnum mission;
    public GameObject prefab;
    public StampCollectible collectible;
    [HideInInspector]
    public AMission instantiatedMission;
}

public class MissionManager : Singleton<MissionManager>
{
    [Header("This will get converted to a Dictionary at runtime.")]
    public List<MissionMapping> missionMappingList;
    private Dictionary<MissionsEnum, MissionMapping> missionMapping;

    private List<InProgressMissionContainer> activeMissions;

    private void Awake()
    {
        InitializeMissionMapping();

        activeMissions = new List<InProgressMissionContainer>();
    }

    private void InitializeMissionMapping()
    {
        missionMapping = new Dictionary<MissionsEnum, MissionMapping>();
        missionMappingList.ForEach(m =>
        {
            missionMapping.Add(m.mission, m);
        });
        if (missionMapping.Count != System.Enum.GetNames(typeof(MissionsEnum)).Length)
        {
            Utils.LogErrorAndStopPlayMode("Expected the number of missions in MissionManager to be equal to the number in MissionsEnum!");
        }
    }

    public GameObject GetMissionPrefabFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.prefab;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }


    public AMission GetInstantiatedMissionFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.instantiatedMission;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }

    public void SetInstantiatedMissionForEnum(MissionsEnum missionEnumValue, AMission mission)
    {
        missionMapping[missionEnumValue].instantiatedMission = mission;
    }

    public StampCollectible GetStampCollectibleFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.collectible;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }

    }

    public AMission StartMission(MissionsEnum missionEnumValue)
    {
        GameObject missionPrefab = GetMissionPrefabFromEnum(missionEnumValue);
        GameObject createdMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity, transform);
        Component missionComponent = createdMission.GetComponent(typeof(AMission));

        if (missionComponent is AMission)
        {
            AMission mission = missionComponent as AMission;

            SetInstantiatedMissionForEnum(missionEnumValue, mission);
            ProgressManager.Instance.AddMission(mission);
            activeMissions.Add(new InProgressMissionContainer(mission, createdMission));
            return mission;
        } 
        else
        {
            Utils.LogErrorAndStopPlayMode($"Expected to find an AMission component on {missionPrefab.name}!");
            return null;
        }
    }

    public InProgressMissionContainer GetActiveMission<T>()
    {
        return activeMissions.Find(m => m.mission is T);
    }

    public void RestartMission(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetInstantiatedMissionFromEnum(missionEnumValue), MissionStatusCode.Started);
    }

    public void CompleteMissionObjective(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetInstantiatedMissionFromEnum(missionEnumValue), MissionStatusCode.Completed);
        Debug.Log("Objective Complete");
    }

    public void EndMission(MissionsEnum missionEnumValue)
    {
        AMission mission = GetInstantiatedMissionFromEnum(missionEnumValue);
        InProgressMissionContainer container = activeMissions.Find(m => m.mission == mission);

        if (container != null)
        {
            Destroy(container.gameObject);
            activeMissions.Remove(container);
            ProgressManager.Instance.UpdateMissionStatus(mission, MissionStatusCode.Closed);

            StampCollectible collectible = GetStampCollectibleFromEnum(missionEnumValue);
            if (collectible != null)
            {
                ProgressManager.Instance.UnlockStampCollectible(collectible);
            }
        }
        else
        {
            Debug.LogError("Tried to end an invalid mission!");
        }
    }

    public GameObject SpawnMissionObject(MissionObject missionObject)
    {
        return Instantiate(missionObject.prefab, missionObject.position, Quaternion.Euler(missionObject.rotation));
    }

    public void DestroyMissionObject(MissionObject missionObject)
    {
        if (missionObject.spawnedInstance)
        {
            Destroy(missionObject.spawnedInstance);
        }
    }
}
