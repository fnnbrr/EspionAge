using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = Utils.GetRequiredComponent<Animator>(this);
    }

    private void OnAnimatorMove()
    {
        GameManager.Instance.GetPlayerTransform().position += anim.deltaPosition;
    }
}
