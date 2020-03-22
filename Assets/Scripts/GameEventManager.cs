using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : Singleton<GameEventManager>
{
    public bool logDebugEvents = false;

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
    private Dictionary<GameEvent, List<GameEventReciever>> recievers;

    public delegate void OnEvent(bool value);
    class GameEventReciever
    {
        public OnEvent action;

        public GameEventReciever(OnEvent _action)
        {
            action = _action;
        }
    }

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

        recievers = new Dictionary<GameEvent, List<GameEventReciever>>();
    }

    public bool CheckEventStatus(GameEvent gameEvent)
    {
        if (eventStatus.TryGetValue(gameEvent, out bool v))
        {
            return v;
        }
        else
        {
            Debug.LogError($"Unknown GameEvent '{gameEvent}' passed into CheckEventStatus! Unexpected behaviour may occur now!");
            return false;
        }
    }

    public void SetEventStatus(GameEvent gameEvent, bool status)
    {
        if (eventStatus.ContainsKey(gameEvent))
        {
            if (eventStatus[gameEvent] != status) // do not register the status changing to the same value!
            {
                if (logDebugEvents) Debug.Log($"Event status for event '{gameEvent}' set to '{status}'");

                eventStatus[gameEvent] = status;
                AlertRecievers(gameEvent, status);
            }
        }
        else
        {
            Debug.LogError($"Unknown GameEvent '{gameEvent}' passed into SetEventStatus! Unexpected behaviour may occur now!");
            return;
        }
    }

    public void SubscribeToEvent(GameEvent gameEvent, OnEvent action)
    {
        if (logDebugEvents) Debug.Log($"Subscriber for event '{gameEvent}'");

        if (recievers.ContainsKey(gameEvent))
        {
            recievers[gameEvent].Add(new GameEventReciever(action));
        }
        else
        {
            recievers.Add(gameEvent, new List<GameEventReciever>() { new GameEventReciever(action) });
        }
    }

    public void AlertRecievers(GameEvent gameEvent, bool status)
    {
        if (recievers.ContainsKey(gameEvent))
        {
            if (logDebugEvents) Debug.Log($"Alerting recievers of event '{gameEvent}'");

            recievers[gameEvent].ForEach(r => {
                r.action.Invoke(status);
            });
        }
    }
}
