using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProgressManager: Singleton<ProgressManager>
{
    // This is done in Mission Manager
    // Keep track of all obtained missions
    // Mission is obtained when from an interactable
    // Missions should also be an interactable which checks if mission is obtained (must talk with group)

    // Dialogue should differ based on if they obtained the mission or not (most likely called in Interactable)
    // Keep track of completed missions
    // Should communicate with Dialogue System on what dialogue should show when interacting with same interactable
    private List<InProgressMissionContainer> completedMissions;

    // Mission manager would have a collectible as a prefab that would be able to unlock upon mission complete
     
    [HideInInspector]
    public bool allStamps = false;

    // List of collectibles needed to be able to add stamps in the inspector
    public List<Collectible> stampCollectibles;
    public Dictionary<Collectible, bool> stampsUnlockStatus;

    void Start()
    {
        foreach(Collectible stamp in stampCollectibles)
        {
            stampsUnlockStatus.Add(stamp, false);
        }
    }

    // Change this to unlock stamp collectible that checks if stamp exists and unlocks it (should be called elsewhere)
    public void UnlockStampCollectible(Collectible stamp)
    {
        if(!stampsUnlockStatus.ContainsKey(stamp))
        {
            Debug.Log("Cannot find stamp collectible from all unlockable stamps");
        }
        else
        {
            stampsUnlockStatus[stamp] = true;
        }
    }

    // Returns all currently unlocked stamps
    public List<Collectible> GetUnlockedStamps()
    {
        List<Collectible> availableStamps = new List<Collectible>();

        foreach(KeyValuePair<Collectible, bool> stamp in stampsUnlockStatus)
        {
            if(stamp.Value)
            {
                availableStamps.Add(stamp.Key);
            }
        }

        return availableStamps;
    }

    // Save/Load Progress (Look into Scriptable Objects)
}
