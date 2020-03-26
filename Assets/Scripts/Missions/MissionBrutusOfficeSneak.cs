using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionBrutusOfficeSneak : AMission
{
    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionBrutusOfficeSneak");
    }

    protected override void Cleanup()
    {
        Debug.Log("Cleaning up mission: MissionBrutusOfficeSneak");
    }
}
