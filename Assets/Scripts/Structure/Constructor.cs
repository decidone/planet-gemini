using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : Production
{
    [SerializeField]
    int maxAmount;
    [SerializeField]
    float cooldown;
    [SerializeField]
    StructureInvenManager sInvenManager;
    [SerializeField]
    RecipeManager rManager;
    [SerializeField]
    GameObject constructor;

    string recipeUI;
    Inventory inventory;
    float prodTimer;
    Dictionary<string, Item> itemDic;
    bool activeUI;
    Recipe recipe;

    void Start()
    {
        inventory = this.GetComponent<Inventory>();
        itemDic = ItemList.instance.itemDic;
        // 레시피 설정하는 부분 임시 설정.
        recipeUI = "Constructor";
        recipe = new Recipe();
    }

    void Update()
    {
        var slot = inventory.SlotCheck(0);
        var slot1 = inventory.SlotCheck(1);

        if (recipe.name != null)
        {
            if (slot.amount >= recipe.amounts[0] && (slot1.amount + recipe.amounts[recipe.amounts.Count - 1]) <= maxAmount)
            {
                Item output = recipe.items[recipe.items.Count - 1];

                if (slot1.item == output || slot1.item == null)
                {
                    prodTimer += Time.deltaTime;
                    if (prodTimer > cooldown)
                    {
                        inventory.Sub(0, recipe.amounts[0]);
                        inventory.SlotAdd(1, output, recipe.amounts[recipe.amounts.Count - 1]);
                        prodTimer = 0;
                    }
                }
                else
                {
                    prodTimer = 0;
                }
            }
            else
            {
                prodTimer = 0;
            }

            if (activeUI)
            {
                sInvenManager.progressBar.SetProgress(prodTimer);
            }
        }
    }

    public void OpenUI()
    {
        if (recipeUI == "Constructor")
        {
            sInvenManager.SetInven(inventory, constructor);
            sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["GoldBar"]);
            sInvenManager.slots[0].SetInputItem(ItemList.instance.itemDic["SilverBar"]);
            sInvenManager.slots[1].outputSlot = true;
            sInvenManager.progressBar.SetMaxProgress(cooldown);
            rManager.recipeBtn.gameObject.SetActive(true);
            rManager.recipeBtn.onClick.RemoveAllListeners();
            rManager.recipeBtn.onClick.AddListener(OpenRecipe);
            activeUI = true;
        }
    }

    public void CloseUI()
    {
        if (recipeUI == "Constructor")
        {
            sInvenManager.ReleaseInven();
            rManager.recipeBtn.onClick.RemoveAllListeners();
            rManager.recipeBtn.gameObject.SetActive(false);
            activeUI = false;
        }
    }

    void OpenRecipe()
    {
        //rManager.SetInven(inventory);
        rManager.OpenUI();
        rManager.SetRecipe("Constructor", this);
    }

    public override void SetRecipe(Recipe _recipe)
    {
        recipe = _recipe;
        Debug.Log(recipe.name);
        sInvenManager.slots[0].ResetOption();
        sInvenManager.slots[0].SetInputItem(recipe.items[0]);
        sInvenManager.slots[1].outputSlot = true;
        sInvenManager.progressBar.SetMaxProgress(recipe.cooldown);
    }
}
