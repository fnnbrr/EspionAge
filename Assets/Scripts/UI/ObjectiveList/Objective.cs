using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objective")]
public class Objective : ScriptableObject 
{
    [TextArea(2, 5)]
    public string line;
}


