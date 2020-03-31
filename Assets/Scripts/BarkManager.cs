using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum BarkEvent
{
    KitchenIdleBark,
    KitchenPlateDroppedNurseReaction,
    KitchenSpottedNurseReaction,
    KitchenLostNurseReaction
}

[System.Serializable]
public class BarkLine
{
    public BarkEvent barkEvent;
    public List<Bark> barks;

    public BarkLine(BarkEvent _barkEvent, List<Bark> _barks)
    {
        barkEvent = _barkEvent;
        barks = _barks;
    }
}

public class BarkManager : Singleton<BarkManager>
{
    [ReorderableList]
    public List<BarkLine> barkLines;
    Dictionary<BarkEvent, BarkLine> barkEventLines;

    private void Awake()
    {
        barkEventLines = new Dictionary<BarkEvent, BarkLine>();

        foreach(BarkLine barkLine in barkLines)
        {
            AddBarkLine(barkLine);
        }
    }

    public void AddBarkLine(BarkLine barkLine)
    {
        if (barkEventLines.ContainsKey(barkLine.barkEvent))
        {
            Debug.LogError("Duplicate Bark tryng to be added");
            return;
        }
        barkEventLines.Add(barkLine.barkEvent, barkLine);
    }

    public List<Bark> GetBarkLines(BarkEvent barkEvent)
    {
        return barkEventLines[barkEvent].barks;
    }
}
