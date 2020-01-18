using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject canRestUI;

    public void EnableCanRestUI(bool toEnable)
    {
        canRestUI.SetActive(toEnable);
    }
}
