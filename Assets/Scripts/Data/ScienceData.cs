using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScienceData
{
    // 아이템 저장량, 업그레이드 상태, 락 상태, 업그레이드 진행 게이지
    public List<int> saveItemCount = new List<int>();
    public int upgradeState;    // 0: 시작 안함, 1: 시작, 2: 완료
    public bool lockCheck;
    public float upgradeTime;
}
