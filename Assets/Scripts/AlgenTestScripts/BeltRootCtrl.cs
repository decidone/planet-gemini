using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltRootCtrl : MonoBehaviour
{
    public Animator[] animArr;
    public float animTime =0;
    bool NewAnim = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        animTime += (Time.deltaTime*100/40);
        if (NewAnim == true && animTime >= 50 && animArr.Length != 0)
            ReSetAnim();
    }

    public void AddAnimArr()
    {
        animArr = GetComponentsInChildren<Animator>();
        NewAnim = true;
    }
    void ReSetAnim()
    {
        for (int index = 0; index < animArr.Length; index++)
        {
            animArr[index].Play("BlendAnim", -1, 0);
        }
        animTime = 0;
        NewAnim = false;
    }
}
