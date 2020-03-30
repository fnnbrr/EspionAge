using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildRootMotionController : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = Utils.GetRequiredComponent<Animator>(this);
    }

    private void OnAnimatorMove()
    {
        transform.parent.position += anim.deltaPosition;
    }

    public void SetBool(string name, bool value)
    {
        anim.SetBool(name, value);
    } 
    
    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    public void SetFloat(string name, float value)
    {
        anim.SetFloat(name, value);
    }

    public void SetAnimationSpeed(float value)
    {
        anim.speed = value;
    }
}
