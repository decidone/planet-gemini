using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    // 건물 공용 스크립트
    // Update처럼 함수 호출하는 부분은 다 하위 클래스에 넣을 것
    // 연결된 구조물 확인 방법 1. 콜라이더, 2. 맵에서 인접 타일 체크

    public int maxHp;
    public int hp;

    public void ConveyorCheck()
    {
        // 연결된 컨베이어 벨트 체크
    }

    public void PipeCheck()
    {
        // 연결된 파이프 체크
    }

    public void StructureCheck()
    {
        // 연결된 건물 체크
    }
}
