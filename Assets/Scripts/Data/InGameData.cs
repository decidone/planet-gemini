using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InGameData
{
    public string fileName;         // 월드 이름
    public string saveDate;         // 저장 날짜
    public int mapSizeIndex;        // 맵 크기
    public int difficultyLevel;     // 레벨
    public int seed;                // 맵 시드
    public bool bloodMoon;          // 블러드문

    public int day;                 // 인게임 날짜
    public bool isDay;              // 낮 / 밤
    public float dayTimer;          // 인게임 시간
    public int dayIndex;            // 24시간을 6등분해서 인덱스로 사용

    public bool wavePlanet;         // 광폭화 행성
    public bool violentDay;         // 광폭화 날

    public int finance;             // 재화
    public int scrap;               // 고철
    public int questIndex;          // 퀘스트 번호

    public string hostPortalName;   // 맵ui 상 포탈 이름
    public string clientPortalName;

    public float hostMapEnergyUseAmount;    // 호스트맵 에너지 사용량
    public float clientMapEnergyUseAmount;  // 클라이언트맵 에너지 사용량

    public bool bloodMoonEventState;    // 블러드문 이벤트 상태
    public int energyOverLimitDay;      // 에너지 오버리밋 일수
    public int waveIndex;               // 웨이브 인덱스
    public float waveDamage;            // 현재 웨이브 데미지
    public float prevWaveDamage;        // 이전 웨이브 총 데미지
    public float currWaveDamage;        // 현재 웨이브 총 데미지
    public float difficultyPercent;     // 웨이브 난이도 증가량
}
