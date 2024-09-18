using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitSaveData
{
    /*
     * 유닛 인덱스, 좌표, 현재 체력, 이동 상태, 패트롤 상태, 이동 시작 좌표, 목적지 좌표
     */

    public int unitIndex;   // 유닛 인덱스
    public SerializedVector3 pos = new SerializedVector3(); // 좌표
    public float hp;        // 체력

    // 유저 유닛
    public int aiState;     // 이동 상태(이동하고 패트롤 채크)
    public int lastState;   // 다른 행동 전 상태
    public bool holdState;  // 홀드 상태
    public bool patrolDir;  // 페트롤 이동 방향
    public SerializedVector3 moveTragetPos = new SerializedVector3();   // 이동해야 하는 좌표
    public SerializedVector3 moveStartPos = new SerializedVector3();    // 이동 시작 좌표
    // 유저 유닛

    // 몬스터
    public int monsterType;     // 몬스터 타입(약, 중, 강, 가디언)
    public bool waveState;      // 웨이브 상태
    public bool waveWaiting;    // 웨이브 대기 상태
    public bool isWaveColonyCallCheck;  // true: 웨이브, false: 콜로니 콜
    public SerializedVector3 wavePos = new SerializedVector3(); // 웨이브, 콜로니콜 위치
    // 몬스터
}
