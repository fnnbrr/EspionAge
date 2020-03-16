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
    public bool shouldFollow = false;
    public Line[] lines;
}
