using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public struct Line
{
    public string id;

    [ResizableTextArea]
    public string text;
}

[CreateAssetMenu(fileName = "New Conversation", menuName = "Conversation")]
public class Conversation : ScriptableObject
{
    [Header("Note: Autoplay Conversation takes precedence over Auto Initiate")]
    public bool autoplayConversation = false;
    public bool autoInitiate = false;
    public Line[] lines;

    public List<string> GetAllSpeakers()
    {
        List<string> allSpeakers = new List<string>();

        foreach(Line line in lines)
        {
            if(!allSpeakers.Contains(line.id))
            {
                allSpeakers.Add(line.id);
            }
        }

        return allSpeakers;
    }
}
