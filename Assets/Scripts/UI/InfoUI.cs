using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    #region Singleton
    public static InfoUI instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of InfoUI found!");
            return;
        }

        instance = this;
    }
    #endregion

    [SerializeField] Text nameText;
    [SerializeField] Text hpText;
    [SerializeField] Text energyText;
    [SerializeField] Text firstBattleText;
    [SerializeField] Text secondBattleText;

    public PlayerStatus player = null;
    public MapObject obj = null;
    public Structure str = null;
    public UnitAi unit = null;
    public MonsterSpawner spawner = null;
    public MonsterAi monster = null;

    public void SetDefault()
    {
        ReleaseInfo();
        nameText.text = "";
        hpText.text = "";
        energyText.text = "";
        firstBattleText.text = "";
        secondBattleText.text = "";
    }

    public void SetPlayerInfo(PlayerStatus _player)
    {
        SetDefault();
        player = _player;
        nameText.text = player.name;
        SetPlayerHp();
        player.onHpChangedCallback += SetPlayerHp;
    }

    public void SetPlayerHp()
    {
        if (player != null)
        {
            hpText.text = player.hp + "/" + player.maxHp;
        }
    }

    public void SetObjectInfo(MapObject _obj)
    {
        SetDefault();
        obj = _obj;
        nameText.text = obj.name;
    }

    public void SetStructureInfo(Structure _str)
    {
        SetDefault();
        str = _str;
        nameText.text = str.buildName;
        if (str.maxLevel > 1)
            nameText.text += " Level " + str.level;
        SetStructureHp();
        str.onHpChangedCallback += SetStructureHp;
        if (str.energyUse)
        {
            energyText.text = "Energy Consume: " + str.energyConsumption;
        }
        else if (str.isEnergyStr && str.energyProduction > 0)
        {
            energyText.text = "Energy Produce: " + str.energyProduction;
        }
    }

    public void SetStructureHp()
    {
        if (str != null)
        {
            hpText.text = str.hp + "/" + str.maxHp;
        }
    }

    public void SetUnitInfo(UnitAi _unit)
    {
        SetDefault();
        unit = _unit;
        nameText.text = unit.name;
        SetUnitHp();
        unit.onHpChangedCallback += SetUnitHp;
        firstBattleText.text = "ATK " + unit.unitCommonData.Damage + " DEF " + unit.unitCommonData.Defense;
        secondBattleText.text = "ATK Delay " + unit.unitCommonData.AttDelayTime + " ATK Range " + unit.unitCommonData.AttackDist;
    }

    public void SetUnitHp()
    {
        if (unit != null)
        {
            hpText.text = unit.hp + "/" + unit.maxHp;
        }
    }

    public void SetSpawnerInfo(MonsterSpawner _spawner)
    {
        SetDefault();
        spawner = _spawner;
        nameText.text = "Spawner Level " + spawner.areaLevel;
    }

    public void SetMonsterInfo(MonsterAi _monster)
    {
        SetDefault();
        monster = _monster;
        nameText.text = monster.name;
    }

    public void ReleaseInfo()
    {
        if (player != null)
        {
            // 플레이어는 디스폰이 없으니까 디스폰 시 릴리즈는 넣지 않음
            player.onHpChangedCallback -= SetPlayerHp;
            player = null;
        }
        else if (obj != null)
        {
            obj = null;
        }
        else if (str != null)
        {
            str.onHpChangedCallback -= SetStructureHp;
            str = null;
        }
        else if (unit != null)
        {
            unit.onHpChangedCallback -= SetUnitHp;
            unit = null;
        }
        else if (spawner != null)
        {
            spawner = null;
        }
        else if (monster != null)
        {
            monster = null;
        }
    }
}
