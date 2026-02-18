using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairEffectFunc : MonoBehaviour
{
    ShaderAnimController animController;

    void Start()
    {
        if (TryGetComponent(out ShaderAnimController anim))
        {
            animController = anim;
        }
    }

    public void EffectStart()
    {
        if (animController != null)
        {
            animController.PlayOnce();
        }
    }

    //public void EffectEnd()
    //{
    //    sprite.enabled = false;
    //}
}
