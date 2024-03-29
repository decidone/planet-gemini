using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class RecipeManager : InventoryManager
{
    public Button recipeBtn;
    [SerializeField]
    GameObject structureInfoUI;
    Dictionary<string, Item> itemDic;
    List<Recipe> recipes;
    Production prod;
    public bool isOpened;

    protected override void Start()
    {
        base.Start();
        itemDic = ItemList.instance.itemDic;

        inputManager = InputManager.instance;
        inputManager.controls.Inventory.Recipe.performed += ctx => SlotClick();
    }

    void SlotClick()
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

    public override void OpenUI()
    {
        inventoryUI.SetActive(true);
        structureInfoUI.SetActive(false);
        isOpened = true;
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public override void CloseUI()
    {
        inventoryUI.SetActive(false);
        itemInfoWindow.CloseWindow();
        structureInfoUI.SetActive(true);
        isOpened = false;
        soundManager.PlayUISFX("CloseUI");
        gameManager.onUIChangedCallback?.Invoke(inventoryUI);
    }

    public void SetRecipeUI(string str, Production _prod)
    {
        inventory.ResetInven();
        prod = _prod;
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);
        for (int i = 0; i < recipes.Count; i++)
        {
            inventory.Add(itemDic[recipes[i].name], 1);
        }
        SetInven(inventory, inventoryUI);
    }

    public List<Recipe> GetRecipeList(string str, Production _prod)
    {
        prod = _prod;
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);
        
        return recipes;
    }
}
