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
    public string npcVoice;
    public Line[] lines;
    public bool isAutoplayed = false;
    public bool shouldFollow = false;
}
