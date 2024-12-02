using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerAi : Production
{
    public enum TowerState
    {
        Waiting,
        Attack,
        AttackDelay
    }

    public TowerData towerData;
    //protected TowerData TowerData { set { towerData = value; } }

    // 유닛 상태 관련
    [HideInInspector]
    public TowerState towerState = TowerState.Waiting;

    [SerializeField]
    protected Animator animator;

    protected float searchTimer = 0f;
    protected float searchInterval = 0.25f; // 딜레이 간격 설정

    List<Item> bulletRecipe;

    protected NetworkObjectPool networkObjectPool;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<CapsuleCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
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
        checkObj = false;
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
        else
            checkObj = true;
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

    public override bool CanTakeItem(Item item)
    {
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
                    inventory.SlotAdd(0, itemProps.item, itemProps.amount);
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

        Dictionary<Item, int> returnDic = new Dictionary<Item, int>();

        if(bulletRecipe.Count > 0)
        {
            foreach (Item item in bulletRecipe)
            {
                returnDic.Add(item, 0);
            }
        }

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
                }
                else
                {
                    returnDic[invenItem.item] += invenItem.amount;
                }
                itemsCount++;
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