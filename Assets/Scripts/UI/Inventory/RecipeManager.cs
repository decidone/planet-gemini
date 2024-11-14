using System.Collections;
using System.Collections.Generic;
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
    Production prod;
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
        inputManager.controls.Inventory.SlotLeftClick.performed += SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed += SlotRightClickHold;
        inputManager.controls.Inventory.Recipe.performed += SlotClick;
    }

    void OnDisable()
    {
        inputManager.controls.Inventory.SlotLeftClick.performed -= SlotLeftClick;
        inputManager.controls.Inventory.SlotRightClickHold.performed -= SlotRightClickHold;
        inputManager.controls.Inventory.Recipe.performed -= SlotClick;
    }

    void SlotClick(InputAction.CallbackContext ctx)
    {
        if (focusedSlot != null)
        {
            if (focusedSlot.item != null)
            {
                for (int i = 0; i < recipes.Count; i++)
                {
                    if(recipes[i].name == focusedSlot.item.name)
                    {
                        prod.SetRecipeServerRpc(i);
                        break;
                    }
                }

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
        if (_prod.GetComponent<UnitFactory>())
        {
            if (GameManager.instance.debug)
            {
                for (int i = 0; i < recipes.Count; i++)
                {
                    Debug.Log(recipes[i].name);

                    inventory.RecipeInvenAdd(itemDic[recipes[i].name], recipes[i].amounts[recipes[i].amounts.Count - 1]);
                }
            }
            else
            {
                TempScienceDb scienceDb = TempScienceDb.instance;
                for (int i = 0; i < recipes.Count; i++)
                {
                    if (scienceDb.scienceNameDb.ContainsKey(recipes[i].name))
                    {
                        inventory.RecipeInvenAdd(itemDic[recipes[i].name], recipes[i].amounts[recipes[i].amounts.Count - 1]);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                inventory.RecipeInvenAdd(itemDic[recipes[i].name], recipes[i].amounts[recipes[i].amounts.Count - 1]);
            }
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

    public List<Recipe> GetRecipeList(string str)
    {
        recipes = new List<Recipe>();
        recipes = RecipeList.instance.GetRecipeInven(str);

        return recipes;
    }
}
