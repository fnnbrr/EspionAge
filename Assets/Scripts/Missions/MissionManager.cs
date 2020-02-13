using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class MissionManager : Singleton<MissionManager>
{
    private List<InProgressMissionContainer> activeMissions;

    // Start is called before the first frame update
    void Start()
    {
        activeMissions = new List<InProgressMissionContainer>();
    }

    public AMission StartMission(GameObject missionPrefab)
    {
        GameObject createdMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity, transform);
        Component missionComponent = createdMission.GetComponent(typeof(AMission));

        if (missionComponent is AMission)
        {
            AMission mission = missionComponent as AMission;

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

    public void EndMission(AMission mission)
    {
        InProgressMissionContainer container = activeMissions.Find(m => m.mission == mission);

        if (container != null)
        {
            Destroy(container.gameObject);
            activeMissions.Remove(container);
        }
        else
        {
            Debug.LogError("Tried to end an invalid mission!");
        }
    }
}
