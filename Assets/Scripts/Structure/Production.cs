using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Production : Structure
{
    // ����(��ź, ����), �۾� �ð�, �۾���, ���, ����ǰ, ������ ����
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

    protected Inventory inventory;
    protected Dictionary<string, Item> itemDic;
    protected float prodTimer;
    protected int fuel;
    protected int maxFuel;
    protected Item output;
    protected Recipe recipe;
    protected List<Recipe> recipes;

    public abstract void OpenUI();
    public abstract void CloseUI();

    protected virtual void Awake()
    {
        inventory = this.GetComponent<Inventory>();
    }

    protected virtual void Start()
    {
        itemDic = ItemList.instance.itemDic;
        recipe = new Recipe();
        output = null;
    }

    public virtual void SetRecipe(Recipe _recipe) { }
    public virtual float GetProgress() { return prodTimer; }
    public virtual float GetFuel() { return fuel; }
    public virtual void OpenRecipe() { }
}
