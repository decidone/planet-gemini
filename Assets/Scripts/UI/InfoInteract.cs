using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoInteract : MonoBehaviour
{
    /*
     * 이름, 현재 체력/최대 체력, 레벨, 설명?
     * 업그레이드 버튼, 철거 버튼 < 기능이 이미 있으니 후순위
     * 선택된 건물의 에너지 생산량, 소모량
     * 공격력, 방어력 등 전투 관련 정보 + 타워 (공격력, 방어력, 공속, 범위)
     * 
     * 업그레이드 버튼은 업글 가능한 상황일 때만 활성화 아닐 땐 회색으로
     */

    [SerializeField] int type;  // 0: 플레이어, 1: 맵 오브젝트, 2: 건물, 3: 유닛, 4: 스포너, 5: 적 유닛

    InfoUI ui;
    PlayerStatus player;
    MapObject obj;
    Structure str;
    UnitAi unit;
    MonsterSpawner spawner;
    MonsterAi monster;

    private void Start()
    {
        ui = InfoUI.instance;

        switch (type)
        {
            case 0:
                player = this.gameObject.GetComponentInParent<PlayerStatus>();
                break;
            case 1:
                obj = this.gameObject.GetComponentInParent<MapObject>();
                break;
            case 2:
                str = this.gameObject.GetComponentInParent<Structure>();
                break;
            case 3:
                unit = this.gameObject.GetComponentInParent<UnitAi>();
                break;
            case 4:
                spawner = this.gameObject.GetComponentInParent<MonsterSpawner>();
                break;
            case 5:
                monster = this.gameObject.GetComponentInParent<MonsterAi>();
                break;
            default:
                break;
        }
    }

    public void Clicked()
    {
        switch (type)
        {
            case 0:
                ui.SetPlayerInfo(player);
                break;
            case 1:
                ui.SetObjectInfo(obj);
                break;
            case 2:
                ui.SetStructureInfo(str);
                break;
            case 3:
                //ui.SetUnitInfo(unit);
                break;
            case 4:
                ui.SetSpawnerInfo(spawner);
                break;
            case 5:
                ui.SetMonsterInfo(monster);
                break;
            default:
                break;
        }
    }
}
