using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stepScript : MonoBehaviour
{

    // Update is called once per frame
    void Step(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
    }
}
