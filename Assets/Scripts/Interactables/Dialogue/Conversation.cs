using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

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
    [ReorderableList]
    public Line[] lines;

    public List<string> GetAllSpeakers()
    {
        return lines.Select(line => line.id).Distinct().ToList();
    }
}