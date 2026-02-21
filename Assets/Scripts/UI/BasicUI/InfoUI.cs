using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text onlyNameText;
    [SerializeField] Text hpText;
    [SerializeField] Button upgradeBtn;
    [SerializeField] Button dicBtn;
    [SerializeField] Button removeBtn;
    [SerializeField] Button chopTreeBtn;

    [Space]
    public Material mat;
    public Material outlineMat;
    public Material animOutlineMat;
    public Material monsterOutlineMat;
    public Material focusedMat;

    [Space]
    public PlayerStatus player = null;
    public MapObject obj = null;
    public Structure str = null;
    public UnitAi unit = null;
    public List<UnitAi> units = null;
    public MonsterSpawner spawner = null;
    public MonsterAi monster = null;

    RemoveBuild removeBuild;
    UpgradeBuild upgradeBuild;

    List<Recipe> selectRecipe;
    Recipe recipe;
    Dictionary<Item, int> upgradeItemDic;

    [SerializeField]
    GameObject unitGroupUI;
    [SerializeField]
    GameObject[] unitSingleUIs;
    [SerializeField]
    Text[] unitCount; 
    Dictionary<(string, int), int> unitDataDic;

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
        selectRecipe = RecipeList.instance.GetRecipeInven("UnitUpgrade");
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
        chopTreeBtn.onClick.RemoveAllListeners();
        chopTreeBtn.gameObject.SetActive(false);
        unitGroupUI.SetActive(false);
    }

    void SetNameText(string txt)
    {
        nameText.text = txt;
        if (txt.Length > 16)
            nameText.fontSize = 12;
        else
            nameText.fontSize = 14;
    }

    public void SetPlayerInfo(PlayerStatus _player)
    {
        SetDefault();
        player = _player;
        SpriteRenderer spriteRenderer = player.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlineMat;
        //nameText.text = player.name;
        SetNameText(player.name);
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
        spriteRenderer.material = outlineMat;
        onlyNameText.text = obj.name;
        chopTreeBtn.gameObject.SetActive(true);
        chopTreeBtn.onClick.AddListener(() => ChopTreeBtnFucn());
    }

    void ChopTreeBtnFucn()
    {
        QuestManager.instance.QuestCompCheck(13);
        SoundManager.instance.PlayUISFX("TreeCut");
        obj.RemoveMapObjRequest();
    }

    public void SetStructureInfo(Structure _str)
    {
        SetDefault();
        str = _str;
        SpriteRenderer spriteRenderer = str.gameObject.GetComponent<SpriteRenderer>();
        focusedMat = spriteRenderer.material;
        if (spriteRenderer.sharedMaterial.shader.name == "Sprites/ShaderAnimated")
        {
            spriteRenderer.material = animOutlineMat;
        }
        else
        {
            spriteRenderer.material = outlineMat;
        }
        //nameText.text = InGameNameDataGet.instance.ReturnName(str.level + 1, str.buildName);
        SetNameText(InGameNameDataGet.instance.ReturnName(str.level + 1, str.buildName));
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
        if (str.GetComponent<Portal>())
        {
            dicBtn.onClick.AddListener(() => InfoDictionary.instance.Search("Portal", true));
        }
        else
        {
            dicBtn.onClick.AddListener(() => InfoDictionary.instance.Search(str));
        }

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
        if (str != null && str.maxHp > 0)
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

    // 단일 클릭 유닛
    public void SetUnitInfo(UnitAi _unit)
    {
        SetDefault();
        unit = _unit;
        units = new List<UnitAi> { unit }; //업글 시 리스트로 넘기기 위해
        SpriteRenderer spriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlineMat;
        //nameText.text = unit.name;
        SetNameText(unit.name);
        SetUnitHp();
        unit.onHpChangedCallback += SetUnitHp;

        if (_unit.CanUpgrade() && ScienceDb.instance.IsLevelExists(_unit.unitCommonData.name, _unit.unitLevel + 2))
        {
            upgradeBtn.gameObject.SetActive(true);
            upgradeBtn.onClick.AddListener(() => 
            {
                UpgradeCostCheckAndPopupSet(units);
            });
        }
        //firstBattleText.text = "ATK " + unit.damage + " DEF " + unit.defense;
        //secondBattleText.text = "ATK Delay " + unit.attackSpeed + " ATK Range " + unit.unitCommonData.AttackDist;
    }

    // 드래그 유닛
    public void SetUnitInfo(List<UnitAi> _units)
    {
        SetDefault();
        units.Clear();
        units = _units;

        if (_units[0].CanUpgrade() && ScienceDb.instance.IsLevelExists(_units[0].unitCommonData.name, _units[0].unitLevel + 2))
        {
            upgradeBtn.gameObject.SetActive(true);
            upgradeBtn.onClick.AddListener(() =>
            {
                UpgradeCostCheckAndPopupSet(_units);
            });
        }

        UnitGroupUISet(_units);
    }

    public void UnitGroupUISet(List<UnitAi> _units)
    {
        unitGroupUI.SetActive(true);

        unitDataDic = new Dictionary<(string, int), int>();
        // (유닛 이름, 레벨), 개수

        for (int i = 0; i < _units.Count; i++)
        {
            var key = (_units[i].unitCommonData.UnitName, _units[i].unitLevel);
            if (unitDataDic.ContainsKey(key))
            {
                unitDataDic[key]++;
            }
            else
            {
                unitDataDic[key] = 1;
            }
        }

        for(int i = 0; i < unitSingleUIs.Length; i++)
        {
            unitSingleUIs[i].gameObject.SetActive(false);
        }

        // 딕셔너리 순회하면서 UI 채우기
        foreach (var kvp in unitDataDic)
        {
            int index = UnitUISellect(kvp.Key);
            unitSingleUIs[index].gameObject.SetActive(true);
            unitCount[index].text = kvp.Value.ToString();
        }
    }

    public void UnitAmountSub((string, int) data)
    {
        int index = UnitUISellect(data);
        unitCount[index].text = (int.Parse(unitCount[index].text) - 1).ToString();
        if (int.Parse(unitCount[index].text) <= 0)
        {
            unitSingleUIs[index].gameObject.SetActive(false);
        }
    }

    int UnitUISellect((string, int) data)
    {
        int index = 0;
        if (data.Item1 == "BounceRobot")
        {
            if (data.Item2 == 0)
            {
                index = 0;
            }
            else
            {
                index = 1;
            }
        }
        else if (data.Item1 == "SentryCopter")
        {
            if (data.Item2 == 0)
            {
                index = 2;
            }
            else
            {
                index = 3;
            }
        }
        else if (data.Item1 == "SpinRobot")
        {
            if (data.Item2 == 0)
            {
                index = 4;
            }
            else
            {
                index = 5;
            }
        }
        else if (data.Item1 == "CorrosionDrone")
        {
            index = 6;
        }
        else if (data.Item1 == "RepairerDrone")
        {   
            index = 7;
        }

        return index;
    }


    void UpgradeCostCheckAndPopupSet(List<UnitAi> _units)
    {
        bool canUpgrade = false;

        for (int i = 0; i < _units.Count;i++)
        {
            if(_units[i].CanUpgrade())
            {
                canUpgrade = true;
                break;
            }
        }

        if (!canUpgrade)
        {
            upgradeBtn.gameObject.SetActive(false);
            return;
        }

        recipe = selectRecipe.Find(r => r.name == _units[0].unitCommonData.UnitName);

        upgradeItemDic = new Dictionary<Item, int>();
        Dictionary<string, Item> itemDic = ItemList.instance.itemDic;

        for (int i = 0; i < recipe.items.Count - 1; i++)
        {
            if (upgradeItemDic.ContainsKey(itemDic[recipe.items[i]]))
            {
                upgradeItemDic[itemDic[recipe.items[i]]] += recipe.amounts[i] * _units.Count;
            }
            else
            {
                upgradeItemDic.Add(itemDic[recipe.items[i]], recipe.amounts[i] * _units.Count);
            }
        }

        Dictionary<Item, int> enoughItemDic = new Dictionary<Item, int>();
        Dictionary<Item, int> notEnoughItemDic = new Dictionary<Item, int>();
        bool isEnough;

        foreach (var kvp in upgradeItemDic)
        {
            Item key = kvp.Key;
            int value = kvp.Value;

            int amount;
            bool hasItem = GameManager.instance.inventory.totalItems.TryGetValue(key, out amount);
            isEnough = hasItem && amount >= value;

            if (!isEnough)
                notEnoughItemDic.Add(key, value - amount);
            else
                enoughItemDic.Add(key, value);
        }

        GameManager.instance.inventoryUiCanvas.GetComponent<PopUpManager>().upgradeConfirm.GetData(enoughItemDic, notEnoughItemDic, "InfoUI");
    }

    public void ConfirmEnd(bool isOk)
    {
        if (isOk)
        {
            foreach (UnitAi unit in units)
            {
                if (!unit || !unit.CanUpgrade())
                {
                    units.Clear();
                    upgradeBtn.gameObject.SetActive(false);
                    SetUnitHp();
                    return;
                }
            }
            GameManager.instance.UnitUpgrade(units);
        }

        units.Clear();
        upgradeBtn.gameObject.SetActive(false);
        SetUnitHp();
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
        if (_spawner.dieCheck) return;

        spawner = _spawner;
        SpriteRenderer spriteRenderer = spawner.gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.material = outlineMat;
        //nameText.text = "Spawner Level " + spawner.spawnerLevel;
        SetNameText("Spawner Level " + spawner.spawnerLevel);
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
        focusedMat = spriteRenderer.material;
        if (spriteRenderer.sharedMaterial.shader.name == "Sprites/MonsterFogVisible")
        {
            spriteRenderer.material = monsterOutlineMat;
        }
        else
        {
            spriteRenderer.material = outlineMat;
        }
        //nameText.text = monster.name;
        SetNameText(monster.name);
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
            spriteRenderer.material = mat;

            player.onHpChangedCallback -= SetPlayerHp;
            player = null;
        }
        else if (obj != null)
        {
            SpriteRenderer spriteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = mat;

            obj = null;
        }
        else if (str != null)
        {
            SpriteRenderer spriteRenderer = str.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = focusedMat;
            focusedMat = null;

            str.onHpChangedCallback -= SetStructureHp;
            str = null;
        }
        else if (unit != null)
        {
            SpriteRenderer spriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = mat;

            unit.onHpChangedCallback -= SetUnitHp;
            unit = null;
        }
        else if (spawner != null)
        {
            SpriteRenderer spriteRenderer = spawner.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = mat;

            spawner.onHpChangedCallback -= SetSpawnerHp;
            spawner = null;
        }
        else if (monster != null)
        {
            SpriteRenderer spriteRenderer = monster.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material = focusedMat;
            focusedMat = null;

            monster.onHpChangedCallback -= SetMonsterHp;
            monster = null;
        }
    }
}
