using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTutorial : AMission
{
    // General Logic Overview:
    // - start out faded out
    // - move player to the spawn point (in her room)
    // - spawn the old person waiting 
    // - mission giver at the end of the hallway
    //   - somehow make the mission giver have a certain conversation
    //   - somehow wait for the player to talk to the the mission giver to end the mission
    // - spawn the breakable vase
    // - fade in
    // - start animation of note going under the door
    //   - this note should be an interactable which will have some text of birdie speaking to herself (reading outloud i guess)
    // - finish mission once we finishing interacting with the mission giver (which will also give us the next mission)
    protected override void Initialize()
    {
        Debug.Log("MissionTutorial Initialize()!");
    }

    protected override void Cleanup()
    {
        Debug.Log("MissionTutorial Cleanup()!");
    }
}
