using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private bool _canRest;

    public bool CanRest
    {
        get { return _canRest; }
        set {
            GameManager.Instance.EnableCanRestUI(value);
            _canRest = value; 
        }
    }
}
