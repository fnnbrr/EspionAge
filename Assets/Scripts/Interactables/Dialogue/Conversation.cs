using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Line
{
    public bool isBirdie;

    [TextArea(2, 5)]
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
