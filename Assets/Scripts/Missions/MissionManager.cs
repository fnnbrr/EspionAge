using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    public List<IMission> activeMissions;

    // Start is called before the first frame update
    void Start()
    {
        activeMissions = new List<IMission>();
    }

    public void StartMission(GameObject missionPrefab)
    {
        GameObject createdMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity, transform);
        if (createdMission.GetComponent(typeof(IMission)) is IMission mission)
        {
            activeMissions.Add(mission);
        }
    }

    public void EndMission(IMission mission)
    {
        activeMissions.Remove(mission);
    }
}
