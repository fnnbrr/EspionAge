using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StampProgressManager : MonoBehaviour
{
    private int numStampsCollected = 0;
    public TextMeshProUGUI text;

    void OnEnable()
    {
        DisplayUpdate();
    }

    void DisplayUpdate()
    {
        numStampsCollected = ProgressManager.Instance.GetNumStampsUnlocked();
        text.SetText(numStampsCollected.ToString());
    }
}
