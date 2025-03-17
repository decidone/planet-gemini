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

    public int day;                 // 인게임 날짜
    public bool isDay;              // 낮 / 밤
    public float dayTimer;          // 인게임 시간
    public int dayIndex;            // 24시간을 6등분해서 인덱스로 사용

    public bool wavePlanet;         // 광폭화 행성
    public float violentValue;      // 광폭화 스택
    public bool violentDayCheck;    // 광폭화 체크
    public bool violentDay;         // 광폭화 날

    public int finance;             // 재화
    public int scrap;               // 고철
    public int questIndex;          // 퀘스트 번호

    public string hostPortalName;   // 맵ui 상 포탈 이름
    public string clientPortalName;
}
