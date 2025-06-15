using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text onlyNameText;
    [SerializeField] Text hpText;
    //[SerializeField] Text energyText;
    //[SerializeField] Text firstBattleText;
    //[SerializeField] Text secondBattleText;
    [SerializeField] Button upgradeBtn;
    [SerializeField] Button dicBtn;
    [SerializeField] Button removeBtn;

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

    RemoveBuild removeBuild;
    UpgradeBuild upgradeBuild;

    #region Singleton
    public static InfoUI instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        removeBuild = GameManager.instance.GetComponent<RemoveBuild>();
        upgradeBuild = GameManager.instance.GetComponent<UpgradeBuild>();
    }

    public void SetDefault()
    {
        ReleaseInfo();
        nameText.text = "";
        onlyNameText.text = "";
        hpText.text = "";
        //energyText.text = "";
        //firstBattleText.text = "";
        //secondBattleText.text = "";
        upgradeBtn.onClick.RemoveAllListeners();
        upgradeBtn.gameObject.SetActive(false);
        dicBtn.onClick.RemoveAllListeners();
        dicBtn.gameObject.SetActive(false);
        removeBtn.onClick.RemoveAllListeners();
        removeBtn.gameObject.SetActive(false);
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
        onlyNameText.text = obj.name;
        removeBtn.gameObject.SetActive(true);
        removeBtn.onClick.AddListener(() => CutDownBtnFucn());
    }

    void CutDownBtnFucn()
    {
        SoundManager.instance.PlayUISFX("TreeCut");
        obj.RemoveMapObjRequest();
    }

    public void SetStructureInfo(Structure _str)
    {
        SetDefault();
        str = _str;
        SpriteRenderer spriteRenderer = str.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlintMat;
        nameText.text = InGameNameDataGet.instance.ReturnName(str.level + 1, str.buildName);
        SetStructureHp();
        str.onHpChangedCallback += SetStructureHp;

        if (!str.isPreBuilding)
        {
            if (!(str.GetComponent<Portal>() || str.GetComponent<ScienceBuilding>()))
            {
                if (str.structureData.MaxLevel != str.level + 1)
                {
                    if (ScienceDb.instance.IsLevelExists(str.buildName, str.level + 2))
                    {
                        // 업그레이드 가능
                        upgradeBtn.gameObject.SetActive(true);
                        upgradeBtn.onClick.AddListener(() => upgradeBuild.UpgradeBtnClicked(str));
                    }
                    else
                    {
                        // 상위 테크 건물은 있는데 아직 연구가 완료되지 않은 경우
                        Debug.Log("need to research next level building");
                    }
                }
            }
        }

        dicBtn.gameObject.SetActive(true);
        dicBtn.onClick.AddListener(() => InfoDictionary.instance.Search(str));

        if (!(str.GetComponent<Portal>() || str.GetComponent<ScienceBuilding>()))
        {
            removeBtn.gameObject.SetActive(true);
            removeBtn.onClick.AddListener(() => removeBuild.RemoveBtnClicked(str));
        }
        //if (str.energyUse)
        //{
        //    energyText.text = "Energy Consume: " + str.energyConsumption;
        //}
        //else if (str.isEnergyStr && str.energyProduction > 0)
        //{
        //    energyText.text = "Energy Produce: " + str.energyProduction;
        //}

        //if (str.gameObject.TryGetComponent<AttackTower>(out AttackTower tower))
        //{
        //    firstBattleText.text = "ATK " + tower.damage;
        //    secondBattleText.text = "ATK Delay " + tower.attDelayTime + " ATK Range " + tower.towerData.AttackDist;
        //}
    }

    public void SetStructureHp()
    {
        if (str != null)
        {
            hpText.text = str.hp + "/" + str.maxHp;
        }
    }

    public void RefreshStrInfo()
    {
        if (str != null)
        {
            SetStructureInfo(str);
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
        //firstBattleText.text = "ATK " + unit.damage + " DEF " + unit.defense;
        //secondBattleText.text = "ATK Delay " + unit.attackSpeed + " ATK Range " + unit.unitCommonData.AttackDist;
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
        nameText.text = "Spawner Level " + spawner.spawnerLevel;
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
        //firstBattleText.text = "ATK " + monster.damage + " DEF " + monster.defense;
        //secondBattleText.text = "ATK Delay " + monster.attackSpeed + " ATK Range " + monster.unitCommonData.AttackDist;
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
