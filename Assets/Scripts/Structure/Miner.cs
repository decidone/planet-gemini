using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// UTF-8 설정
public class Miner : Production
{
    protected override void Start()
    {
        base.Start();
        Init();
        isMainSource = true;
        StartCoroutine(EfficiencyCheckLoop());
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            if (conn != null && conn.group != null && conn.group.efficiency > 0)
            {
                if (output != null && slot.Item2 < maxAmount)
                {
                    OperateStateSet(true);
                    prodTimer += Time.deltaTime;
                    if (prodTimer > effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount))
                    {
                        soundManager.PlaySFX(gameObject, "structureSFX", "Miner");

                        if (slot.Item2 + minerCellCount <= maxAmount)
                        {
                            if (IsServer)
                            {
                                inventory.Add(output, minerCellCount);
                                Overall.instance.OverallProd(output, minerCellCount);
                            }
                            prodTimer = 0;
                        }
                        else
                        {
                            int addAmount = maxAmount - slot.Item2;
                            if (IsServer)
                            {
                                inventory.Add(output, addAmount);
                                Overall.instance.OverallProd(output, addAmount);
                            }
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

            if (IsServer && slot.Item2 > 0 && outObj.Count > 0 && !itemSetDelay)
            {
                int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(output);
                SendItem(itemIndex);
                //SendItem(output);
            }
        }
    }

    public override void CheckSlotState(int slotindex)
    {
        // update에서 검사해야 하는 특정 슬롯들 상태를 인벤토리 콜백이 있을 때 미리 저장
        slot = inventory.SlotCheck(0);
    }

    protected override void OnClientConnectedCallback(ulong clientId)
    { 
        base.OnClientConnectedCallback(clientId);
        InitStartServerRpc();
    }

    [ServerRpc]
    void InitStartServerRpc()
    {
        InitStartClientRpc();
    }

    [ClientRpc]
    void InitStartClientRpc()
    {
        if(!IsServer)
            Init();
    }

    void Init()
    {
        Map map;
        if(isInHostMap)
            map = GameManager.instance.hostMap;
        else
            map = GameManager.instance.clientMap;

        int x;
        int y;

        if (sizeOneByOne)
        {
            x = Mathf.FloorToInt(this.gameObject.transform.position.x);
            y = Mathf.FloorToInt(this.gameObject.transform.position.y);

            if (map.IsOnMap(x, y))
            {
                Resource resource = map.GetCellDataFromPos(x, y).resource;
                if (resource != null && resource.type == "ore")
                {
                    Item item = resource.item;
                    if (item != null)
                    {
                        SetResource(item, resource.level, resource.efficiency, 1);
                    }
                }
            }
        }
        else
        {
            x = Mathf.FloorToInt(this.gameObject.transform.position.x - 0.5f);
            y = Mathf.FloorToInt(this.gameObject.transform.position.y - 0.5f);

            Dictionary<Item, (int, float, int)> mapItems = new Dictionary<Item, (int, float, int)>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (map.IsOnMap(x, y))
                    {
                        Resource resource = map.GetCellDataFromPos(x + j, y + i).resource;
                        if (resource != null && resource.type == "ore")
                        {
                            Item item = resource.item;
                            if (item != null)
                            {
                                if(level + 1 >= resource.level)
                                {
                                    if(!mapItems.ContainsKey(item))
                                        mapItems.Add(item, (1, resource.efficiency, resource.level));
                                    else
                                    {
                                        var existingValue = mapItems[item];
                                        var updatedValue = (existingValue.Item1 + 1, existingValue.Item2, existingValue.Item3);
                                        mapItems[item] = updatedValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if(mapItems.Count > 0)
            {
                Item highestItem = null;
                int highestQuantity = 0;
                float highestEfficiency = 0;
                int highestLevel = 0;

                foreach (var data in mapItems)
                {
                    Item itemData = data.Key;
                    int countData = data.Value.Item1;
                    float efficiencyData = data.Value.Item2;
                    int levelData = data.Value.Item3;

                    if (highestQuantity < countData || (highestQuantity == countData && highestEfficiency < efficiencyData))
                    {
                        highestItem = itemData;
                        highestQuantity = countData;
                        highestEfficiency = efficiencyData;
                        highestLevel = levelData;
                    }
                }

                if(highestItem != null)
                {
                    SetResource(highestItem, highestLevel, highestEfficiency, highestQuantity);
                }
            }
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);

        float productionTime = effiCooldown - ((overclockOn ? effiCooldown * overclockPer / 100 : 0) + effiCooldownUpgradeAmount);
        float productionPerMin = minerCellCount * (60 / productionTime);
        sInvenManager.progressBar.SetMaxProgress(productionTime);
        sInvenManager.SetCooldownText(productionTime, FormatFloat(productionPerMin));

        sInvenManager.slots[0].outputSlot = true;
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.ReleaseInven();
    }

    void SetResource(Item item, int _level, float _efficiency, int _minerCellCount)
    {
        int mineLevel = level + 1;
        if (mineLevel >= _level)
        {
            output = item;
            if (mineLevel == 1)
            {
                cooldown = _efficiency * 1.5f;
            }
            else if (mineLevel == 2)
            {
                cooldown = _efficiency;
            }
            else
            {
                cooldown = _efficiency * 0.8f;
            }

            if (conn != null && conn.group != null)
                EfficiencyCheck();
            else
                effiCooldown = cooldown;
            minerCellCount = _minerCellCount;
            FactoryOverlay();
        }
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Miner")
            {
                ui = list;
            }
        }
    }

    protected override void NonOperateStateSet(bool isOn)
    {
        if (animController == null) return;

        if (isOn)
        {
            if (!animController.isInitialized)
            {
                this.GetComponent<SpriteRenderer>().material = shaderAnimatedMat;
                animController.Refresh();
            }
            else
            {
                animController.Resume();
            }
        }
        else
        {
            animController.Pause();
        }
    }
}
