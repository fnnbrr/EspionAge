using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : Singleton<GameEventManager>
{
    /// <summary>
    /// NOTES:
    /// - Do not make duplicate names below!
    /// </summary>
    public enum GameEvent
    {
        TutorialActive,
        TutorialComplete,
        KitchenMissionComplete,
        HasThrownSomething
    }
    private readonly Dictionary<GameEvent, bool> defaultValues = new Dictionary<GameEvent, bool>()
    {
        {GameEvent.TutorialActive, false },
        {GameEvent.TutorialComplete, false },
        {GameEvent.KitchenMissionComplete, false },
        {GameEvent.HasThrownSomething, false }
    };

    private Dictionary<GameEvent, bool> eventStatus; 

    private void Awake()
    {
        eventStatus = new Dictionary<GameEvent, bool>();

        foreach(GameEvent gameEvent in (GameEvent[]) System.Enum.GetValues(typeof(GameEvent)))
        {
            // TryGetValue will make v the default value for that type:
            // "If the key is not found, then the value parameter gets the appropriate default value for the type TValue; 
            // for example, 0 (zero) for integer types, false for Boolean types, and null for reference types."
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.trygetvalue?view=netframework-4.8
            defaultValues.TryGetValue(gameEvent, out bool v);
            eventStatus.Add(gameEvent, v);
        }
    }

    public bool CheckEventStatus(GameEvent gameEvent)
    {
        if (eventStatus.TryGetValue(gameEvent, out bool v))
        {
            return v;
        }
        else
        {
            Debug.LogError($"Unknown GameEvent {gameEvent} passed into CheckEventStatus! Unexpected behaviour may occur now!");
            return false;
        }
    }

    public void SetEventStatus(GameEvent gameEvent, bool status)
    {
        if (eventStatus.ContainsKey(gameEvent))
        {
            eventStatus[gameEvent] = status;
        }
        else
        {
            Debug.LogError($"Unknown GameEvent {gameEvent} passed into SetEventStatus! Unexpected behaviour may occur now!");
            return;
        }
    }
}
