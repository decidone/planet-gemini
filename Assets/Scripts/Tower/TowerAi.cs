using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerAi : Production
{
    public TowerData towerData;
    //protected TowerData TowerData { set { towerData = value; } }

    protected float searchTimer = 0f;
    protected float searchInterval; // 딜레이 간격 설정

    List<Item> bulletRecipe;

    protected NetworkObjectPool networkObjectPool;

    // 타워
    bool[] increasedTower;
    // 0 공격력, 1 공격 속도

    public float attDelayTime;
    public float damage;

    protected override void Awake()
    {
        base.Awake();
        attDelayTime = towerData.AttDelayTime;
        damage = towerData.Damage;
        increasedStructure = new bool[2];
        col = GetComponent<CapsuleCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        searchInterval = 0.25f;
        networkObjectPool = NetworkObjectPool.Singleton;
        if (!structureData.EnergyUse[level])
        {
            recipes = rManager.GetRecipeList("Tower", this);

            bulletRecipe = new List<Item>();

            foreach (Recipe recipeData in recipes)
            {
                if(recipeData.name == structureData.FactoryName)
                {
                    recipe = recipeData;
                    foreach (string itemsName in recipe.items)
                    {
                        bulletRecipe.Add(itemDic[itemsName]);
                    }
                }
            }
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetProd(this);

        sInvenManager.slots[0].SetInputItem(bulletRecipe);

        sInvenManager.progressBar.gameObject.SetActive(false);
        sInvenManager.energyBar.gameObject.SetActive(false);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        sInvenManager.progressBar.gameObject.SetActive(true);
        sInvenManager.energyBar.gameObject.SetActive(true);
        sInvenManager.ReleaseInven();
    }


    //protected override void CheckNearObj(Vector3 startVec, Vector3 endVec, int index, Action<GameObject> callback)
    //{
    //    RaycastHit2D[] hits = Physics2D.RaycastAll(this.transform.position + startVec, endVec, 1f);

    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        Collider2D hitCollider = hits[i].collider;

    //        if (hitCollider.CompareTag("Factory") && hitCollider.GetComponent<Structure>().isSetBuildingOk &&
    //            hits[i].collider.gameObject != this.gameObject)
    //        {
    //            nearObj[index] = hits[i].collider.gameObject;
    //            callback(hitCollider.gameObject);
    //            break;
    //        }
    //    }
    //}

    protected override IEnumerator SetOutObjCoroutine(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);

        if (obj.TryGetComponent(out Structure structure) && !structure.isMainSource)
        {
            if (obj.TryGetComponent(out BeltCtrl belt))
            {
                if (obj.GetComponentInParent<BeltGroupMgr>().nextObj == this.gameObject)
                {
                    StartCoroutine(SetInObjCoroutine(obj));
                    yield break;
                }
            }
        }
    }

    protected override void RepairEnd()
    {
        base.RepairEnd();
        animator.SetBool("isDie", false);
    }

    public override void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Tower")
            {
                ui = list;
            }
        }
    }

    public override void OnFactoryItem(Item item)
    {
        if (IsServer)
        {
            inventory.SlotAdd(0, item, 1);
        }
    }

    public override bool CanTakeItem(Item item)
    {
        if (isInvenFull || energyUse) return false;

        var slot = inventory.SlotCheck(0);
        if (slot.item == null)
        {
            foreach (Item _recipe in bulletRecipe)
            {
                if (item == _recipe)
                    return true;
            }
        }
        else if (slot.item == item && slot.amount < 99)
            return true;

        return false;
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        if (IsServer)
        {
            foreach (Item _recipe in bulletRecipe)
            {
                if(itemProps.item == _recipe)
                {
                    inventory.SlotAdd(0, itemProps.item, itemProps.amount);
                    break;
                }
            }
        }

        itemProps.itemPool.Release(itemProps.gameObject);
    }

    public override Dictionary<Item, int> PopUpItemCheck()
    {
        if (structureData.EnergyUse[level])
        {
            return null;
        }
        else
        {
            Dictionary<Item, int> returnDic = new Dictionary<Item, int>();

            int itemsCount = 0;
            //다른 슬롯의 같은 아이템도 개수 추가하도록
            for (int i = 0; i < inventory.space; i++)
            {
                var invenItem = inventory.SlotCheck(i);

                if (invenItem.item != null && invenItem.amount > 0)
                {
                    if (!returnDic.ContainsKey(invenItem.item))
                    {
                        returnDic.Add(invenItem.item, invenItem.amount);
                        itemsCount++;
                    }
                    else
                    {
                        returnDic[invenItem.item] += invenItem.amount;
                    }

                    if (itemsCount > 5)
                        break;
                }
            }

            if (returnDic.Count > 0)
            {
                return returnDic;
            }
            else
                return null;
        }
    }

    public override void IncreasedStructureCheck()
    {
        base.IncreasedStructureCheck();

        increasedTower = ScienceDb.instance.IncreasedStructureCheck(1);
        // 0 공격력, 1 공격속도

        if (increasedTower[0])
        {
            damage = towerData.UpgradeDamage;
        }
        if (increasedTower[1])
        {
            attDelayTime = towerData.UpgradeAttDelayTime;
        }
    }
}