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

        // 여기 업글 시 수정되게
        CheckPos();
        setModel.sprite = modelNum[dirNum + level];
    }
}
