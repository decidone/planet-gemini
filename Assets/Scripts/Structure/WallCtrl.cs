using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCtrl : Structure
{
    //void Start()
    //{
    //    setModel = GetComponent<SpriteRenderer>();
    //}

    protected override void Update()
    {
        base.Update();
        SetDirNum();
    }

    protected override void SetDirNum()
    {
        setModel.sprite = modelNum[dirNum + level];
        CheckPos();
    }
}
