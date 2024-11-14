using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    /*
     * 플레이어 위치(벡터값, 행성), hp, 휴대용 채굴기 상태
     */

    public SerializedVector3 pos = new SerializedVector3();
    public float hp;
    public bool isPlayerInHostMap;
    public bool isPlayerInMarket;

    public bool tankOn;
    public float tankHp;
    public float tankMaxHp;

    public bool clientFirstConnection;  // 클라이언트가 접속했는지
}
