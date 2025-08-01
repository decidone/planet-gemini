using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// UTF-8 설정
public class RecipeManager : InventoryManager
{
    public Button recipeBtn;
    [SerializeField]
    GameObject structureInfoUI;
    Dictionary<string, Item> itemDic;
    List<Recipe> recipes;
    public Production prod;
    public bool isOpened;
    string buildingName;

    protected override void Start()
    {
        base.Start();
        itemDic = ItemList.instance.itemDic;
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        //inputManager.controls.Inventory.SlotLeftClick.performed += SlotLeftClick;
        //inputManager.controls.Inventory.SlotRightClickHold.performed += SlotRightClickHold;
        inputManager.controls.Inventory.Recipe.performed += SlotClick;
    }

    void OnDisable()
    {
        //inputManager.controls.Inventory.SlotLeftClick.performed -= SlotLeftClick;
        //inputManager.controls.Inventory.SlotRightClickHold.performed -= SlotRightClickHold;
        inputManager.controls.Inventory.Recipe.performed -= SlotClick;
    }

    void SlotClick(InputAction.CallbackContext ctx)
    {
        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                prod.SetRecipeServerRpc(focusedSlot.slotNum);
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
        focusedSlot = null;
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
        buildingName = str;
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);

        int[] slotNums = new int[recipes.Count];
        Item[] itemIndexs = new Item[recipes.Count];
        int[] itemAmounts = new int[recipes.Count];

        if (_prod.GetComponent<UnitFactory>())
        {
            if (GameManager.instance.debug)
            {
                for (int i = 0; i < recipes.Count; i++)
                {
                    slotNums[i] = i;
                    if (recipes[i].name != "UICancel")
                        itemAmounts[i] = recipes[i].amounts[recipes[i].amounts.Count - 1];
                    else
                        itemAmounts[i] = 0;
                    itemIndexs[i] = itemDic[recipes[i].name];
                }
            }
            else
            {
                ScienceDb scienceDb = ScienceDb.instance;
                for (int i = 0; i < recipes.Count; i++)
                {
                    if (recipes[i].name == "UICancel")
                    {
                        slotNums[i] = i;
                        itemAmounts[i] = 0;
                        itemIndexs[i] = itemDic[recipes[i].name];
                    }
                    else if (scienceDb.scienceNameDb.ContainsKey(recipes[i].name))
                    {
                        slotNums[i] = i;
                        itemAmounts[i] = recipes[i].amounts[recipes[i].amounts.Count - 1];
                        itemIndexs[i] = itemDic[recipes[i].name];
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                slotNums[i] = i;
                if (recipes[i].name != "UICancel")
                    itemAmounts[i] = recipes[i].amounts[recipes[i].amounts.Count - 1];
                else
                    itemAmounts[i] = 0;
                itemIndexs[i] = itemDic[recipes[i].name];
            }
        }

        inventory.NonNetSlotsAdd(slotNums, itemIndexs, itemAmounts, recipes.Count);
        SetInven(inventory, inventoryUI);
    }

    public List<Recipe> GetRecipeList(string str, Production _prod)
    {
        prod = _prod;
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);
        
        return recipes;
    }

    public List<Recipe> GetRecipeList(string str)
    {
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);

        return recipes;
    }
}
