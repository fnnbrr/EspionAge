using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InProgressMissionContainer
{
    public AMisson mission;
    public GameObject gameObject;

    public InProgressMissionContainer(AMisson _mission, GameObject _gameObject)
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

    public AMisson StartMission(GameObject missionPrefab)
    {
        GameObject createdMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity, transform);
        Component missionComponent = createdMission.GetComponent(typeof(AMisson));

        if (missionComponent is AMisson)
        {
            AMisson mission = missionComponent as AMisson;

            activeMissions.Add(new InProgressMissionContainer(mission, createdMission));
            return mission;
        } 
        else
        {
            Utils.LogErrorAndStopPlayMode($"Expected to find an IMission component on {missionPrefab.name}!");
            return null;
        }
    }

    public InProgressMissionContainer GetActiveMission<T>()
    {
        return activeMissions.Find(m => m.mission is T);
    }

    public void EndMission(AMisson mission)
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
