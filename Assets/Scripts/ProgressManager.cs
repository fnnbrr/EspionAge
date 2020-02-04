using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager: Singleton<ProgressManager>
{
    // Should communicate with Dialogue System on what dialogue should show when interacting with same interactable
    // Needed to communicate with the dialogue system
    public List<InProgressMissionContainer> completedMissions;
     
    [HideInInspector]
    public bool allStampsUnlocked = false;

    public List<Collectible> stampCollectibles;                        // List of collectibles needed to be able to add stamps in the inspector
    public Dictionary<Collectible, bool> stampsUnlockStatus;           // Use a dictinoary to keep track of what stamps are unlocked


    void Start()
    {
        stampsUnlockStatus = new Dictionary<Collectible, bool>();

        // All stamp collectibles are set to locked (false) at beginning of game
        foreach(Collectible stamp in stampCollectibles)
        {
            stampsUnlockStatus.Add(stamp, false);
        }

        completedMissions = new List<InProgressMissionContainer>();
    }

  
    public void UnlockStampCollectible(Collectible stamp)
    {
        if(!stampsUnlockStatus.ContainsKey(stamp))
        {
            Debug.Log("Cannot find stamp collectible from all unlockable stamps");
            return; 
        }
        else
        {
            stampsUnlockStatus[stamp] = true;
            Debug.Log("Unlocked " + stamp.collectibleName);

            if(HasUnlockedAllStamps())
            {
                allStampsUnlocked = true;
            }
        }
    }


    public bool HasUnlockedAllStamps()
    {
        return !stampsUnlockStatus.ContainsValue(false);
    }


    // Returns a list of all currently unlocked stamps
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

    // To be called in Mission Manager when mission is completed
    public void AddCompletedMission(InProgressMissionContainer completedMission)
    {
        completedMissions.Add(completedMission);
    }

    // TODO: Save/Load Progress (Look into Scriptable Objects)
}
