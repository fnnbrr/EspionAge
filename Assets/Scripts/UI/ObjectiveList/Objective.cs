using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "New Objective", menuName = "Objective")]
public class Objective : ScriptableObject 
{
    public string[] tags;
    public string[] objectives;
}


