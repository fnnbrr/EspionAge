using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stamp", menuName = "Collectible/Stamp")] //Will put in child class when there are more collectibles
public class Collectible: ScriptableObject
{
    public string collectibleName;

    // Sprite used for stamp. Will move this to a child class for stamp once another collectible is made
    public Sprite image;

}