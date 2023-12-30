using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFactory : Production
{
    public Vector2[] nearPos = new Vector2[8];
    public Vector2 spawnPos;
    bool isSetPos;

    List<GameObject> unitObjList;

    GameObject spawnUnit;
    string setUnitName;

    protected override void Start()
    {
        base.Start();
        isSetPos = false;
        isGetLine = true;
        unitObjList = UnitList.instance.unitList;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!isPreBuilding)
        {
            var slot = inventory.SlotCheck(0);
            var slot1 = inventory.SlotCheck(1);
            var slot2 = inventory.SlotCheck(2);

            if (recipe.name != null)
            {
                if (slot.amount >= recipe.amounts[0] && slot1.amount >= recipe.amounts[1]
                    && slot2.amount >= recipe.amounts[2])
                {
                    prodTimer += Time.deltaTime;
                    if (prodTimer > recipe.cooldown)
                    {
                        bool spawnPosExist = UnitSpawnPosFind();

                        if (spawnPosExist)
                        {
                            inventory.Sub(0, recipe.amounts[0]);
                            inventory.Sub(1, recipe.amounts[1]);
                            inventory.Sub(2, recipe.amounts[2]);

                            SetUnit();
                            SpawnUnit();
                            prodTimer = 0;
                        }
                    }
                }
                else
                {
                    prodTimer = 0;
                }
            }
        }
    }

    public override void OpenUI()
    {
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);
        sInvenManager.progressBar.SetMaxProgress(cooldown);

        rManager.recipeBtn.gameObject.SetActive(true);
        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.onClick.AddListener(OpenRecipe);

        sInvenManager.InvenInit();
        if (recipe.name != null)
            SetRecipe(recipe);

        if (isSetPos)
            LineRendererSet(spawnPos);
    }

    public override void CloseUI()
    {
        sInvenManager.ReleaseInven();

        rManager.recipeBtn.onClick.RemoveAllListeners();
        rManager.recipeBtn.gameObject.SetActive(false);

        base.DestroyLineRenderer();
    }

    public override void OpenRecipe()
    {
        rManager.OpenUI();
        rManager.SetRecipeUI("UnitFactory", this);
    }

    public override void SetRecipe(Recipe _recipe)
    {
        if (recipe.name != null && recipe != _recipe)
        {
            sInvenManager.EmptySlot();
        }
        recipe = _recipe;
        sInvenManager.ResetInvenOption();
        sInvenManager.slots[0].SetInputItem(itemDic[recipe.items[0]]);
        sInvenManager.slots[1].SetInputItem(itemDic[recipe.items[1]]);
        sInvenManager.slots[2].SetInputItem(itemDic[recipe.items[2]]);
        sInvenManager.slots[3].SetInputItem(itemDic[recipe.items[3]]);
        sInvenManager.slots[3].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
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

    protected override void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

        if (nearPos[index] != null)
            nearPos[index] = this.transform.position + startVec + endVec;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
                hits[i].collider.gameObject != this.gameObject)
            {
                nearObj[index] = hits[i].collider.gameObject;
                callback(hitCollider.gameObject);
                break;
            }
        }
    }

    public bool UnitSpawnPosFind()
    {
        bool spawnPosExist = false;
        for (int i = 0; i < nearPos.Length; i++)
        {
            if (nearObj[i] != null)
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

    public void UnitSpawnPosSet(Vector2 _spawnPos)
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
        unit.transform.position = this.transform.position;
        UnitAi unitAi = unit.GetComponent<UnitAi>();
        unitAi.MovePosSet(spawnPos, 0, true);
    }

    public override void DestroyLineRenderer()
    {
        base.DestroyLineRenderer();
        isSetPos = false;
    }
}