using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// UTF-8 설정
public abstract class Production : Structure
{
    // 연료(석탄, 전기), 작업 시간, 작업량, 재료, 생산품, 아이템 슬롯
    [SerializeField]
    protected GameObject ui;
    [SerializeField]
    protected StructureInvenManager sInvenManager;
    [SerializeField]
    protected RecipeManager rManager;
    [SerializeField]
    protected int maxAmount;
    [SerializeField]
    protected float cooldown;

    protected GameObject canvas;
    protected Inventory inventory;
    protected Dictionary<string, Item> itemDic;
    protected float prodTimer;
    protected int fuel;
    protected int maxFuel;
    protected Item output;
    protected Recipe recipe;
    protected List<Recipe> recipes;
    protected int invenCount;
    protected Dictionary<Item, int> invenSlotDic;

    [SerializeField]
    protected GameObject lineObj;
    [HideInInspector]
    public LineRenderer lineRenderer;
    protected Vector3 startLine;
    protected Vector3 endLine;
    public bool isGetLine;

    public virtual void SetRecipe(Recipe _recipe) { }
    public virtual float GetProgress() { return prodTimer; }
    public virtual float GetFuel() { return fuel; }
    public virtual void OpenRecipe() { }
    public virtual void GetUIFunc() { }

    protected override void Awake()
    {
        base.Awake();
        inventory = this.GetComponent<Inventory>();
        isGetLine = false;
        isStorageBuilding = false;
    }

    protected virtual void Start()
    {
        itemDic = ItemList.instance.itemDic;
        recipe = new Recipe();
        output = null;

        GameManager gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();
        CheckPos();
    }

    protected override void Update()
    {
        base.Update();

        if (isSetBuildingOk)
        {
            for (int i = 0; i < nearObj.Length; i++)
            {
                if (nearObj[i] == null && sizeOneByOne)
                {
                    CheckNearObj(checkPos[i], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
                else if (nearObj[i] == null && !sizeOneByOne) 
                {
                    int dirIndex = i / 2;
                    CheckNearObj(startTransform[indices[i]], directions[dirIndex], i, obj => StartCoroutine(SetOutObjCoroutine(obj)));
                }
            }
        }

        if (!isPreBuilding && checkObj)
        {
            if (!isMainSource && inObj.Count > 0 && !itemGetDelay)
                GetItem();
        }
    }

    public virtual void OpenUI()
    {
        isUIOpened = true;
    }

    public virtual void CloseUI()
    {
        isUIOpened = false;
        GameManager.instance.CheckAndCancelFocus(this);
    }

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
                belt.FactoryPosCheck(GetComponentInParent<Structure>());
            }
            else
            {
                outSameList.Add(obj);
                StartCoroutine(OutCheck(obj));
            }
            outObj.Add(obj);
            StartCoroutine(UnderBeltConnectCheck(obj));
        }
        else
            checkObj = true;
    }

    protected override IEnumerator OutCheck(GameObject otherObj)
    {
        yield return new WaitForSeconds(0.1f);

        if (otherObj.TryGetComponent(out Structure otherFacCtrl))
        {
            if (otherObj.GetComponent<Production>())
                yield break;

            if (otherFacCtrl.outSameList.Contains(this.gameObject) && outSameList.Contains(otherObj))
            {
                StartCoroutine(SetInObjCoroutine(otherObj));
                outObj.Remove(otherObj);
                outSameList.Remove(otherObj);
                Invoke("RemoveSameOutList", 0.1f);
            }
        }
    }

    public override void OnFactoryItem(ItemProps itemProps)
    {
        for (int i = 0; i < inventory.space; i++)
        {
            if (itemDic[recipe.items[i]] == itemProps.item)
            {
                inventory.SlotAdd(i, itemProps.item, itemProps.amount);
            }
        }
        base.OnFactoryItem(itemProps);
    }

    public override void OnFactoryItem(Item item)
    {
        for (int i = 0; i < inventory.space; i++)
        {
            if (itemDic[recipe.items[i]] == item)
            {
                inventory.SlotAdd(i, item, 1);
            }
        }
    }

    protected override void SubFromInventory()
    {
        inventory.Sub(inventory.space - 1, 1);
    }

    public virtual bool CanTakeItem(Item item) 
    {
        if (recipe == null || recipe.items == null)
            return false;

        for (int i = 0; i < inventory.space - 1; i++)
        {
            var slot = inventory.SlotCheck(i);
            if (itemDic[recipe.items[i]] == item && slot.amount < 99)
                return true;
        }
        return false;
    }

    public override bool CheckOutItemNum()
    {
        var slot = inventory.SlotCheck(inventory.space - 1);
        if (slot.amount > 0)
            return true;
        else
            return false;
    }

    public virtual (Item, int) QuickPullOut()
    {
        var slot = inventory.SlotCheck(inventory.space - 1);
        if (slot.amount > 0)
            inventory.Sub(inventory.space - 1, slot.amount);
        return slot;
    }

    protected override void GetItem()
    {
        itemGetDelay = true;

        if (inObj[getItemIndex].TryGetComponent(out BeltCtrl belt) && belt.isItemStop)
        {
            if (belt.itemObjList.Count > 0 && CanTakeItem(belt.itemObjList[0].item))
            {
                OnFactoryItem(belt.itemObjList[0]);
                belt.itemObjList[0].transform.position = this.transform.position;
                belt.isItemStop = false;
                belt.itemObjList.RemoveAt(0);
                belt.beltGroupMgr.groupItem.RemoveAt(0);
                belt.ItemNumCheck();

                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                Invoke("DelayGetItem", structureData.SendDelay);
            }
            else
            {
                getItemIndex++;
                if (getItemIndex >= inObj.Count)
                    getItemIndex = 0;

                DelayGetItem();
                return;
            }
        }
        else
        {
            getItemIndex++;
            if (getItemIndex >= inObj.Count)
                getItemIndex = 0;

            DelayGetItem();
            return;
        }        
    }

    protected override void AddInvenItem()
    { 
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);

            if (invenItem.item != null && invenItem.amount > 0)
            {
                playerInven.Add(invenItem.item, invenItem.amount);
            }
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck()
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

    public bool UnloadItem(Item item)
    {
        bool canUnload = false;
        for (int i = 0; i < inventory.space; i++)
        {
            var invenItem = inventory.SlotCheck(i);
            if (invenItem.item == item && invenItem.amount > 0)
            {
                canUnload = true;
                inventory.Sub(i, 1);
                break;
            }
        }
        return canUnload;
    }

    public void LineRendererSet(Vector2 endPos)
    {
        if (endPos != Vector2.zero && lineRenderer == null)
        {
            startLine = new Vector3(transform.position.x, transform.position.y, -1);
            endLine = new Vector3(endPos.x, endPos.y, -1);

            GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
            lineRenderer = currentLine.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startLine);
            lineRenderer.SetPosition(1, endLine);
        }
    }

    public virtual void DestroyLineRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
            lineRenderer = null;
        }
    }

    public void ResetLine(Vector2 endPos)
    {
        DestroyLineRenderer();
        LineRendererSet(endPos);
    }
}
