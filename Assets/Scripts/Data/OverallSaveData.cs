using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OverallSaveData
{
    public Dictionary<int, int> itemsProduction = new Dictionary<int, int>();          // 생산한 아이템 종류별 총합
    public Dictionary<int, int> itemsConsumption = new Dictionary<int, int>();         // 소모한 아이템 종류별 총합
    public Dictionary<int, int> purchasedItems = new Dictionary<int, int>();           // 구매한 아이템 종류별 총합
    public Dictionary<int, int> soldItems = new Dictionary<int, int>();                // 판매/분해한 아이템 종류별 총합
    public Dictionary<int, int> itemsFromHostToClient = new Dictionary<int, int>();    // 호스트 행성에서 클라이언트 행성으로 전송한 아이템 종류별 총합
    public Dictionary<int, int> itemsFromClientToHost = new Dictionary<int, int>();    // 클라이언트 행성에서 호스트 행성으로 전송한 아이템 종류별 총합
    public int spawnerDestroyCount;   // 스포너 파괴 카운트 (스포너 단계에 따른 분류는 따로 하지 않음)
    public int spawnerBountyReceived; // 스포너 파괴 보상 카운트
    public int monsterKillCount;      // 몬스터 킬 카운트 (마찬가지로 몬스터 종류에 따른 분류는 따로 하지 않음)
    public int monsterBountyReceived; // 몬스터 킬 보상 카운트
}
