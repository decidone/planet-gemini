using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltRootCtrl : MonoBehaviour
{
    public Animator[] anim;

    float animTime =0;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        animTime += (Time.deltaTime*100/40);
    }

    public void ResetAnimArr()
    {
        anim = GetComponentsInChildren<Animator>();

        anim[anim.Length - 1].Play("BlendAnim", -1, animTime % 1);
    }
}
