using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTutorial : AMission
{
    public Vector3 playerStartPosition;
    public MissionObject vase;
    private LoudObject vaseLoudObject;

    public MissionObject note;

    public MissionObject distractingSenior;
    private NPCInteractable distractingSeniorNPCInteractable;
    private float distractingSeniorDefaultBoundaryRadius;

    // General Logic Overview:
    // * start out faded out
    // * move player to the spawn point (in her room)
    // * spawn the old person waiting 
    // ~ mission giver at the end of the hallway
    //   ~ somehow make the mission giver have a certain conversation
    //   ~ somehow wait for the player to talk to the the mission giver to end the mission
    // * spawn the breakable vase
    // * fade in
    // - start animation of note going under the door
    //   - this note should be an interactable which will have some text of birdie speaking to herself (reading outloud i guess)
    // * finish mission once we finishing interacting with the mission giver (which will also give us the next mission)
    protected override void Initialize()
    {
        Debug.Log("MissionTutorial Initialize()!");

        // Force fade out
        UIManager.Instance.InstantFadeOut();

        // Move player to a set position
        GameManager.Instance.GetPlayerTransform().position = playerStartPosition;

        // Spawn the old person waiting, that'll annoy us once we drop the vase
        //  set the boundary radius to 0 so they do not follow us
        distractingSenior.spawnedInstance = MissionManager.Instance.SpawnMissionObject(distractingSenior);
        distractingSenior.spawnedInstance.tag = Constants.TAG_ENEMY;
        distractingSeniorNPCInteractable = distractingSenior.spawnedInstance.GetComponent<NPCInteractable>();
        distractingSeniorDefaultBoundaryRadius = distractingSeniorNPCInteractable.boundaryRadius;
        distractingSeniorNPCInteractable.boundaryRadius = 0f;

        // Toggle the event in the EventManager
        GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, true);
        // The old person waiting should already be in the world and be set up with a conditional conversation that'll end this mission
        //  the logic there should be based on this GameEvent enum, and for now just end the current mission, since we know itll be the first thing
        //  - cleaner way otherwise would possibily be having a mapping of some Mission enum to class types, where we can do something like:
        //      - FindObjectOfType<class_type>(), the pass it into the MissionManager as per usual

        // Spawn the vase (or is it already there?) and init the values there
        vase.spawnedInstance = MissionManager.Instance.SpawnMissionObject(vase);
        vaseLoudObject = vase.spawnedInstance.GetComponentInChildren<LoudObject>();
        vaseLoudObject.OnHit += OnVaseDrop;

        // Fade in
        UIManager.Instance.FadeIn();

        // Start the note spawning and start the animation
        note.spawnedInstance = MissionManager.Instance.SpawnMissionObject(note);
    }

    private void OnVaseDrop()
    {
        vaseLoudObject.OnHit -= OnVaseDrop;
        distractingSeniorNPCInteractable.boundaryRadius = distractingSeniorDefaultBoundaryRadius;
        distractingSenior.spawnedInstance.tag = Constants.TAG_NONE;
    }

    protected override void Cleanup()
    {
        // Here we have checks for all the instances specifically because this can be called on App shutdown
        //  this means its possible for some Singletons to have already been garbage collected by the time we get here
        Debug.Log("MissionTutorial Cleanup()!");

        // Despawn the old person waiting for us

        if (GameEventManager.Instance)
        {
            // Toggle the event in the EventManager
            GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, false);
        }

        if (MissionManager.Instance)
        {
            // Goodbye distracting senior!
            MissionManager.Instance.DestroyMissionObject(distractingSenior);

            // Despawn the vase (if it isnt already going to be there?)
            MissionManager.Instance.DestroyMissionObject(vase);

            // Delete the note if it still exists
            MissionManager.Instance.DestroyMissionObject(note);
        }
    }
}
