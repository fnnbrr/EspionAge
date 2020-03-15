using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODSFX : MonoBehaviour
{
    // Used and called from Animation events mostly
    void FMODPlayOneShot(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
    }
}
