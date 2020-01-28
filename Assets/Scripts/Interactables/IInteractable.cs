using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application
{
    public interface IInteractable
    {
        void OnInteract();
        void FaceInteractable(GameObject player);
    }
}
