using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestParent : MonoBehaviour, TestInterface
{
     public virtual void testMethod(string text)
     {
         print("hello bitch!!! testmethod");
     }
}
