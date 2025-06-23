using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairEffectFunc : MonoBehaviour
{
    SpriteRenderer sprite;
    Animator animator;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void EffectStart()
    {
        sprite.enabled = true;
        animator.Play("EffectStart", -1, 0);
    }

    public void EffectEnd()
    {
        sprite.enabled = false;
    }
}
