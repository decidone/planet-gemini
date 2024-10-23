using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPortal : Structure
{
    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider2D>();
    }

    public override void ColliderTriggerOnOff(bool isOn) { }

}
