using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTutorial : AMission
{
    public Vector3 playerStartPosition;
    public MissionObject vase;
    public MissionObject note;

    // General Logic Overview:
    // * start out faded out
    // * move player to the spawn point (in her room)
    // - spawn the old person waiting 
    // ~ mission giver at the end of the hallway
    //   ~ somehow make the mission giver have a certain conversation
    //   ~ somehow wait for the player to talk to the the mission giver to end the mission
    // - spawn the breakable vase
    // * fade in
    // - start animation of note going under the door
    //   - this note should be an interactable which will have some text of birdie speaking to herself (reading outloud i guess)
    // - finish mission once we finishing interacting with the mission giver (which will also give us the next mission)
    protected override void Initialize()
    {
        Debug.Log("MissionTutorial Initialize()!");

        // Force fade out
        UIManager.Instance.InstantFadeOut();

        // Move player to a set position
        GameManager.Instance.GetPlayerTransform().position = playerStartPosition;

        // Spawn the old person waiting, that'll annoy us once we drop the vase

        // Toggle the event in the EventManager
        // ~> GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, true);
        // The old person waiting should already be in the world and be set up with a conditional conversation that'll end this mission
        //  the logic there should be based on this GameEvent enum, and for now just end the current mission, since we know itll be the first thing
        //  - cleaner way otherwise would possibily be having a mapping of some Mission enum to class types, where we can do something like:
        //      - FindObjectOfType<class_type>(), the pass it into the MissionManager as per usual

        // Spawn the vase (or is it already there?) and init the values there
        if (MissionManager.Instance)
        {
            vase.spawnedInstance = MissionManager.Instance.SpawnMissionObject(vase);
        }

        // Fade in
        UIManager.Instance.FadeIn();

        // Start the note spawning and start the animation
        if (MissionManager.Instance)
        {
            note.spawnedInstance = MissionManager.Instance.SpawnMissionObject(note);
        }
    }

    protected override void Cleanup()
    {
        Debug.Log("MissionTutorial Cleanup()!");

        // Despawn the old person waiting for us

        // Toggle the event in the EventManager
        // ~> GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, false);

        if (MissionManager.Instance)
        {
            // Despawn the vase (if it isnt already going to be there?)
            MissionManager.Instance.DestroyMissionObject(vase);

            // Delete the note if it still exists
            MissionManager.Instance.DestroyMissionObject(note);
        }
    }
}
