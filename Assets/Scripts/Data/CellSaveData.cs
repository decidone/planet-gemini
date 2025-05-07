using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CellSaveData
{
    //public int x;
    //public int y;
    //public int biome;       // 0: plain, 1: forest, 2: desert, 3: snow, 4: frozen, 5: lake, 6: cliff
    //public int tile;        // 바이옴 먼저 받고 해당 바이옴 내 타일들과 비교해서 몇 번째 타일인지 체크 후 저장
    //public int exTile;      // 바이옴이 절벽으로 바뀐 경우 경계면 배경타일을 로드해오기 위해서 저장
    //public int resource;    // 0: Coal, 1: Stone, 2: CopperOre, 3: IronOre, 4: AdamantiteOre, 5 ~ 8: CrudeOil(5: 왼쪽 아래, 6: 오른쪽 아래, 7: 왼쪽 위, 8: 오른쪽 위)
    //public int obj;         // 번호 어쩌구. 기존 파괴된 오브젝트를 저장하는 방식은 삭제 필요
    public int[] dataList = new int[7];     // 위 데이터들을 한 배열에 넣어둠
    public List<int> buildable = new List<int>();     // 0: none, 1: miner, 2: pump, 3: extractor, 4: ScienceBuilding, 5: PortalObj

    //public GameObject structure;    // 이건 따로 저장하기 때문에 필요없을듯. 오히려 저장하면 중복저장으로 꼬일 것 같음

    //public bool isCorrupted;  // 스포너 위치를 저장하기 때문에 필요없음
    //public GameObject corruptionObj;  // 어차피 커럽션 내에는 건설이 불가능해서 데이터가 꼬일 일 없으니 그냥 스포너 젠 시킬 때 랜덤생성

    //public bool spawnArea = false;    // 맵 생성 과정에서 스폰지점 근처에 절벽 바이옴을 넣지 않기 위해 있는 변수이므로 저장할 필요 없음
}