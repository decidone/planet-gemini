using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class Miner : Production
{
    int minerCellCount;

    protected override void Start()
    {
        base.Start();
        Init();
        isMainSource = true;
    }

    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            if (output != null && slot.amount < maxAmount)
            {
                prodTimer += Time.deltaTime;
                if (prodTimer > cooldown)
                {
                    if(slot.amount + minerCellCount <= maxAmount)
                    {
                        inventory.Add(output, minerCellCount);
                        prodTimer = 0;
                    }
                    else
                    {
                        int addAmount = maxAmount - slot.amount;
                        inventory.Add(output, addAmount);
                        prodTimer = 0;
                    }
                }
            }

            if (slot.amount > 0 && outObj.Count > 0 && !itemSetDelay && checkObj)
            {
                SendItem(output);
            }
        }
    }

    void Init()
    {
        Map map = GameManager.instance.map;
        int x;
        int y;

        if (sizeOneByOne)
        {
            x = Mathf.FloorToInt(this.gameObject.transform.position.x);
            y = Mathf.FloorToInt(this.gameObject.transform.position.y);

            if (map.IsOnMap(x, y))
            {
                Resource resource = map.mapData[x][y].resource;
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
                        Resource resource = map.mapData[x + j][y + i].resource;
                        if (resource != null && resource.type == "ore")
                        {
                            Item item = resource.item;
                            if (item != null)
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
                    SetResource(highestItem, highestLevel, highestEfficiency, highestQuantity);
            }
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);
        
        sInvenManager.slots[0].outputSlot = true;
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();
    }

    void SetResource(Item item, int _level, float _efficiency, int _minerCellCount)
    {
        if(level >= _level)
        {
            output = item;
            cooldown = _efficiency;
            minerCellCount = _minerCellCount;
        }
    }

    public override void TempBuilCooldownSet() 
    {
        cooldown += 3;
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
}
