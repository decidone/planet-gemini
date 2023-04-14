using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeList : MonoBehaviour
{
    Dictionary<string, List<Recipe>> recipeDic;
    List<Recipe> recipes;
    Dictionary<string, Item> itemDic;
    List<Item> items;
    List<int> amounts;
    Recipe recipe;
    // 레시피 양식 확정되면 json으로 만들어서 저장/관리하고 여기서 불러와서 사용

    #region Singleton
    public static RecipeList instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of recipeList found!");
            return;
        }

        instance = this;
        recipeDic = new Dictionary<string, List<Recipe>>();
    }
    #endregion

    void Start()
    {
        itemDic = ItemList.instance.itemDic;

        // Furnace
        recipes = new List<Recipe>();
        items = new List<Item> { itemDic["Gold"], itemDic["GoldBar"] };
        amounts = new List<int> { 1, 1 };
        recipe = new Recipe("GoldBar", items, amounts, 3f);
        recipes.Add(recipe);

        items = new List<Item> { itemDic["Silver"], itemDic["SilverBar"] };
        amounts = new List<int> { 2, 3 };
        recipe = new Recipe("SilverBar", items, amounts, 3f);
        recipes.Add(recipe);

        recipeDic.Add("Furnace", recipes);

        // Constructor
        recipes = new List<Recipe>();
        items = new List<Item> { itemDic["Gold"], itemDic["GoldBar"] };
        amounts = new List<int> { 2, 1 };
        recipe = new Recipe("GoldBar", items, amounts, 3f);
        recipes.Add(recipe);

        items = new List<Item> { itemDic["Silver"], itemDic["SilverBar"] };
        amounts = new List<int> { 1, 1 };
        recipe = new Recipe("SilverBar", items, amounts, 3f);
        recipes.Add(recipe);

        recipeDic.Add("Constructor", recipes);

        // Assembler
        recipes = new List<Recipe>();
        items = new List<Item> { itemDic["Gold"], itemDic["GoldBar"], itemDic["Coal"] };
        amounts = new List<int> { 2, 1, 2 };
        recipe = new Recipe("Coal", items, amounts, 3f);
        recipes.Add(recipe);

        items = new List<Item> { itemDic["Silver"], itemDic["SilverBar"], itemDic["Gold"] };
        amounts = new List<int> { 1, 1, 2 };
        recipe = new Recipe("Gold", items, amounts, 3f);
        recipes.Add(recipe);

        recipeDic.Add("Assembler", recipes);
    }

    public List<Recipe> GetRecipeInven(string str)
    {
        recipes = recipeDic[str];
        return recipes;
    }
}
