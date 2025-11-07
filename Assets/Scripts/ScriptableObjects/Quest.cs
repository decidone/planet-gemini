using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Data/Quest")]
public class Quest : ScriptableObject
{
    /*
     * 0: 텔레포트, 1: 아이템 전송
     * 10: 아이템 구매, 11: 아이템 판매
     * 20: 건물 건설, 21: 아이템 생산, 22: 유닛 생산
     * 30: 적 스포너 파괴, 31: 적 유닛 처치
     * 40: 연구 완료
     * 
     * 따로 감지가 필요한 항목
     * 텔레포트, 건물 건설, 유닛 생산, 연구 완료, 스포너 파괴, 적 유닛 처치
     * 나머지는 오버롤에 항목별 콜백으로 감지
     */

    [Header("Common")]
    [Tooltip("0: 텔레포트, 1: 아이템 전송 \n10: 아이템 구매, 11: 아이템 판매 \n20: 건물 건설, 21: 아이템 생산, 22: 유닛 생산 \n30: 적 스포너 파괴, 31: 적 유닛 처치 \n40: 특정 연구 완료 \n50: 웨이브 클리어")]
    public int type = -1;
    [TextArea(3, 10)] public string title;
    [TextArea(3, 10)] public string description;

    [Space]
    [Header("Requirement")]
    public Item item;
    public StructureData strData;
    public UnitCommonData unitData;
    public int amount = -1;              // 아이템 수량 뿐만 아니라 적 유닛 처치 수 등 수량 관련에서는 다 이거 사용

    [Space]
    [Header("Teleport")]
    public int destination = -1;         // 0: 행성 간 이동, 1: 마켓

    [Space]
    [Header("Science")]
    public int scienceTech = -1;

    public bool hasDicLink = false;
    public string dicKeyword = string.Empty;
}
