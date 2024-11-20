using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPortal : Structure
{
    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider2D>();
        visionPos = new Vector3(transform.position.x, transform.position.y + 1, 0);
    }

    public override void ColliderTriggerOnOff(bool isOn) { }
}
