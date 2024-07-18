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
    [SerializeField] Button btn;

    [Space]
    public Material outlintMat;
    public Material noOutlineMat;

    [Space]
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
        btn.onClick.RemoveAllListeners();
        btn.gameObject.SetActive(false);
    }

    public void SetPlayerInfo(PlayerStatus _player)
    {
        SetDefault();
        player = _player;
        SpriteRenderer spriteRenderer = player.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
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
        SpriteRenderer spriteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
        nameText.text = obj.name;
        btn.gameObject.SetActive(true);
        btn.onClick.AddListener(obj.RemoveMapObjRequest);
    }

    public void SetStructureInfo(Structure _str)
    {
        SetDefault();
        str = _str;
        SpriteRenderer spriteRenderer = str.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
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

        if (str.gameObject.TryGetComponent<AttackTower>(out AttackTower tower))
        {
            firstBattleText.text = "ATK " + tower.towerData.Damage;
            secondBattleText.text = "ATK Delay " + tower.towerData.AttDelayTime + " ATK Range " + tower.towerData.AttackDist;
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
        SpriteRenderer spriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
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
        SpriteRenderer spriteRenderer = spawner.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
        nameText.text = "Spawner Level " + spawner.areaLevel;
        SetSpawnerHp();
        spawner.onHpChangedCallback += SetSpawnerHp;
    }

    public void SetSpawnerHp()
    {
        if (spawner != null)
        {
            hpText.text = spawner.hp + "/" + spawner.maxHp;
        }
    }

    public void SetMonsterInfo(MonsterAi _monster)
    {
        SetDefault();
        monster = _monster;
        SpriteRenderer spriteRenderer = monster.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
        nameText.text = monster.name;
        SetMonsterHp();
        monster.onHpChangedCallback += SetMonsterHp;
        firstBattleText.text = "ATK " + monster.unitCommonData.Damage + " DEF " + monster.unitCommonData.Defense;
        secondBattleText.text = "ATK Delay " + monster.unitCommonData.AttDelayTime + " ATK Range " + monster.unitCommonData.AttackDist;
    }

    public void SetMonsterHp()
    {
        if (monster != null)
        {
            hpText.text = monster.hp + "/" + monster.maxHp;
        }
    }

    public void ReleaseInfo()
    {
        if (player != null)
        {
            SpriteRenderer spriteRenderer = player.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            player.onHpChangedCallback -= SetPlayerHp;
            player = null;
        }
        else if (obj != null)
        {
            SpriteRenderer spriteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            obj = null;
        }
        else if (str != null)
        {
            SpriteRenderer spriteRenderer = str.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            str.onHpChangedCallback -= SetStructureHp;
            str = null;
        }
        else if (unit != null)
        {
            SpriteRenderer spriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            unit.onHpChangedCallback -= SetUnitHp;
            unit = null;
        }
        else if (spawner != null)
        {
            SpriteRenderer spriteRenderer = spawner.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            spawner.onHpChangedCallback -= SetSpawnerHp;
            spawner = null;
        }
        else if (monster != null)
        {
            SpriteRenderer spriteRenderer = monster.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = noOutlineMat;

            monster.onHpChangedCallback -= SetMonsterHp;
            monster = null;
        }
    }
}
