using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeManager : InventoryManager
{
    public Button recipeBtn;
    [SerializeField]
    GameObject structureInfoUI;
    Dictionary<string, Item> itemDic;
    List<Recipe> recipes;
    Production prod;

    protected override void Start()
    {
        base.Start();
        itemDic = ItemList.instance.itemDic;
    }

    protected override void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (focusedSlot != null)
            {
                if (focusedSlot.item != null)
                {
                    prod.SetRecipe(recipes[focusedSlot.slotNum]);
                    focusedSlot = null;
                    CloseUI();
                }
            }
        }
    }

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);
        structureInfoUI.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        structureInfoUI.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public void SetRecipeUI(string str, Production _prod)
    {
        // 인벤 초기화 기능 추가할 것
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);
        for (int i = 0; i < recipes.Count; i++)
        {
            inventory.Add(itemDic[recipes[i].name], 1);
        }
        SetInven(inventory, inventoryUI);
        prod = _prod;
    }
}
