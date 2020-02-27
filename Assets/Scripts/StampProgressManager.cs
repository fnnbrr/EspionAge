using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StampProgressManager : MonoBehaviour
{
    public TextMeshProUGUI text;

    void OnEnable()
    {
        DisplayUpdate();
    }

    void DisplayUpdate()
    {
        text.SetText(ProgressManager.Instance.GetNumStampsUnlocked().ToString());
    }
}
