using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODSFX : MonoBehaviour
{
    void FMODPlayOneShot(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path);
        Debug.Log($"Triggered OneShot: {path}");
    }
}
