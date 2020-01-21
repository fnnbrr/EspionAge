using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Application
{
    public interface IItem
    {
        void OnTriggerStay(Collider other);

        void AcquireItem(Transform player);

        void DropItem();

        void Effect();

    }
}
