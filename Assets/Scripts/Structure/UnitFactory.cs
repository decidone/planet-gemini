using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitFactory : Production
{
    public Vector2[] nearPos = new Vector2[8];
    public Vector2 spawnPos;
    bool isSetPos = false;

    List<GameObject> unitObjList;

    GameObject spawnUnit;
    string setUnitName;
    
    protected override void Start()
    {
        base.Start();
        isGetLine = true;
        unitObjList = UnitList.instance.unitList;
        StartCoroutine(EfficiencyCheck());
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (recipe.name != null)
            {
                if (conn != null && conn.group != null && conn.group.efficiency > 0 && gameManager.playerUnitLimit > gameManager.playerUnitAmount)
                {
                    if (slot.Item2 >= recipe.amounts[0] && slot1.Item2 >= recipe.amounts[1]
                    && slot2.Item2 >= recipe.amounts[2])
                    {
                        OperateStateSet(true);
                        prodTimer += Time.deltaTime;
                        if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                        {
                            bool spawnPosExist = UnitSpawnPosFind();

                            if (spawnPosExist)
                            {
                                if (IsServer)
                                {
                                    Overall.instance.OverallConsumption(slot.Item1, recipe.amounts[0]);
                                    Overall.instance.OverallConsumption(slot1.Item1, recipe.amounts[1]);
                                    Overall.instance.OverallConsumption(slot2.Item1, recipe.amounts[2]);

                                    inventory.SlotSubServerRpc(0, recipe.amounts[0]);
                                    inventory.SlotSubServerRpc(1, recipe.amounts[1]);
                                    inventory.SlotSubServerRpc(2, recipe.amounts[2]);

                                    SetUnit();
                                    SpawnUnit();
                                }

                                soundManager.PlaySFX(gameObject, "structureSFX", "Structure");
                                prodTimer = 0;
                            }
                        }
                    }
                    else
                    {
                        OperateStateSet(false);
                        prodTimer = 0;
                    }
                }
                else
                {
                    OperateStateSet(false);
                    prodTimer = 0;
                }
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
        slot1 = inventory.SlotCheck(1);
        slot2 = inventory.SlotCheck(2);
    }

    public override void CheckInvenIsFull(int slotIndex)
    {
        // output slot을 제외하고 나머지 슬롯이 가득 차 있는지 체크
        for (int i = 0; i < 3; i++)
        {
            if (inventory.SlotAmountCheck(i) < inventory.maxAmount)
            {
                isInvenFull = false;
                return;
            }
        }

        isInvenFull = true;
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe, recipeIndex);

        if (isSetPos)
            LineRendererSet(spawnPos);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();

        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.gameObject.SetActive(false);

        base.DestroyLineRenderer();
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        ConnectedSetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ConnectedSetServerRpc()
    {
        if (isSetPos)
        {
            ConnectedSetClientRpc(spawnPos);
        }
    }

    [ClientRpc]
    void ConnectedSetClientRpc(Vector3 pos)
    {
        if (IsServer)
            return;

        UnitSpawnPosSetServerRpc(pos);
    }

    public override void OpenRecipe()
    {
        rManager.OpenUI();
        rManager.SetRecipeUI("UnitFactory", this);
    }

    public override void SetRecipe(Recipe _recipe, int index)
    {
        recipe = _recipe;
        recipeIndex = index;
        sInvenManager.ResetInvenOption();
        cooldown = recipe.cooldown;
        effiCooldown = cooldown;
        sInvenManager.progressBar.SetMaxProgress(effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount));

        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[0].SetNeedAmount(recipe.amounts[0]);
        sInvenManager.slots[1].SetInputItem(itemDic[recipe.items[1]]);
        sInvenManager.slots[1].SetNeedAmount(recipe.amounts[1]);
        sInvenManager.slots[2].SetInputItem(itemDic[recipe.items[2]]);
        sInvenManager.slots[2].SetNeedAmount(recipe.amounts[2]);
        sInvenManager.slots[3].SetInputItem(itemDic[recipe.items[3]]);
        sInvenManager.slots[3].SetNeedAmount(recipe.amounts[3]);
        sInvenManager.slots[3].outputSlot = true;

        if (recipe.name == "Tank" || recipe.name == "UICancel")
            sInvenManager.UnitIconSet(false);
        else
            sInvenManager.UnitIconSet(true);
    }

    public void CooldownTextSet()
    {
        sInvenManager.UnitLimitText();
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "UnitFactory")
            {
                ui = list;
            }
        }
    }

    protected override void CheckNearObj(int index, Action<GameObject> callback)
    {
        int nearX = (int)transform.position.x + twoDirections[index, 0];
        int nearY = (int)transform.position.y + twoDirections[index, 1];
        Cell cell = GameManager.instance.GetCellDataFromPosWithoutMap(nearX, nearY);
        if (cell == null)
            return;

        if (nearPos[index] != null)
            nearPos[index] = new Vector2(nearX, nearY);

        GameObject obj = cell.structure;
        if (obj != null)
        {
            if (obj.CompareTag("Factory"))
            {
                nearObj[index] = obj;
                callback(obj);
            }
        }
    }

    public bool UnitSpawnPosFind()
    {
        bool spawnPosExist = false;
        for (int i = 0; i < nearPos.Length; i++)
        {
            if (nearObj[i] != null && !nearObj[i].GetComponent<BeltCtrl>())
                continue;
            else
            {
                spawnPosExist = true;
                if(!isSetPos)
                    spawnPos = nearPos[i];
                break;
            }
        }

        return spawnPosExist;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnitSpawnPosSetServerRpc(Vector2 _spawnPos)
    {
        UnitSpawnPosSetClientRpc(_spawnPos);
    }

    [ClientRpc]
    public void UnitSpawnPosSetClientRpc(Vector2 _spawnPos)
    {
        isSetPos = true;
        spawnPos = _spawnPos;
    }

    void SetUnit()
    {
        if (spawnUnit == null || (spawnUnit != null && (setUnitName != itemDic[recipe.items[3]].name)))
        {
            foreach (GameObject obj in unitObjList)
            {
                obj.TryGetComponent(out UnitAi unitAi);
                if (itemDic[recipe.items[3]].name == obj.name)
                {
                    spawnUnit = obj;
                    setUnitName = unitAi.unitName;
                }
            }
        }
    }

    void SpawnUnit()
    {
        GameObject unit = Instantiate(spawnUnit);
        Vector3 spawnSet = transform.position + ((Vector3)spawnPos - transform.position).normalized * 1.5f;
        unit.transform.position = spawnSet;
        NetworkObject networkObject = unit.GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned) networkObject.Spawn(true);

        //NetworkObjManager.instance.NetObjAdd(unit);
        UnitAi unitAi = unit.GetComponent<UnitAi>();
        unitAi.AStarSet(isInHostMap);
        //unit.transform.position = this.transform.position;
        unitAi.MovePosSetServerRpc(spawnPos, 0, true);
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
        isSetPos = false;
    }

    public override StructureSaveData SaveData()
    {
        StructureSaveData data = base.SaveData();

        if (isSetPos)
        {
            data.connectedStrPos.Add(Vector3Extensions.FromVector3(spawnPos));
        }

        return data;
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        setModel.sprite = strImg[isOn ? 1 : 0];
    }

    protected override void FactoryOverlay()
    {
        if (!gameManager.overlayOn)
        {
            overlay.UIReset();
        }
        else
        {
            if (recipe.name != null && itemDic[recipe.items[3]])
                overlay.UISet(itemDic[recipe.items[3]]);
        }
    }
}