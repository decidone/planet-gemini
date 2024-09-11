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

    public int day;                 // 인게임 날짜
    public bool isDay;              // 낮 / 밤
    public float dayTimer;          // 인게임 시간
    public int dayIndex;            // 24시간을 6등분해서 인덱스로 사용

    public float violentValue;      // 광폭화 스택
    public bool violentDay;         // 광폭화 날인지
}
