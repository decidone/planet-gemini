using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnerSaveData
{
    /*
     * 일단 스포너도 맵 젠에 붙어있는 거긴 한데 맵을 불러오는 경우 스포너는 시드젠을 하지 않고 저장된 데이터 기반으로 불러옴
     * 
     * 좌표, 레벨, 가지고 있는 몹 인덱스, 가지고 있는 몹 세이브 데이터, 체력, 자극형 웨이브가 진행중이였을 경우 진행상황
     * 
     * 몹 인덱스와 세이브 데이터를 분리해둔건 몹을 스폰시키지 않고 리스트로만 가지고 있는 상태와 몹이 이미 스폰돼서 전투 등 변동이 생겼을 경우를 분리하기 위해 넣음
     * 이건 얼마든지 변동사항이 생길 수 있음
     */
}