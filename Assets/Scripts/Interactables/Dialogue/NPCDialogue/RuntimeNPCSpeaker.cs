using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeNPCSpeaker : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string voicePath;

    public static int uniqueNumber = 0;

    private const int MAX_ITERATIONS = 10;

    private string currentSpeakerId;

    public static string GetUniqueSpeakerId(GameObject gameObject)
    {
        int currentIteration = 0;

        string uniqueId;
        do
        {
            uniqueId = $"{gameObject.name}-{uniqueNumber++}";
            if (currentIteration++ > MAX_ITERATIONS)
            {
                Utils.LogErrorAndStopPlayMode($"Could not find unique speaker id for {gameObject.name}!");
                break;
            }
        }
        while (DialogueManager.Instance && DialogueManager.Instance.HasSpeaker(uniqueId));

        return uniqueId;
    }

    private void OnEnable()
    {
        // Known current issue: 
        //  if this is put on an object that is in the game world OnStart, this will fail because it initializes before the DialogueManager's Awake
        currentSpeakerId = GetUniqueSpeakerId(gameObject);
        DialogueManager.Instance.AddSpeaker(new SpeakerContainer(currentSpeakerId, gameObject, voicePath));
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.RemoveSpeaker(currentSpeakerId);
        }
    }
}
