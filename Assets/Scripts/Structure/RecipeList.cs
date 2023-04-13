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
    // ������ ��� Ȯ���Ǹ� json���� ���� ����/�����ϰ� ���⼭ �ҷ��ͼ� ���

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
        recipes = new List<Recipe>();
        recipeDic = new Dictionary<string, List<Recipe>>();
    }
    #endregion

    void Start()
    {
        itemDic = ItemList.instance.itemDic;

        items = new List<Item> { itemDic["Gold"], itemDic["GoldBar"] };
        amounts = new List<int> { 2, 1 };
        recipe = new Recipe("GoldBar", items, amounts, 3f);
        recipes.Add(recipe);

        items = new List<Item> { itemDic["Silver"], itemDic["SilverBar"] };
        amounts = new List<int> { 1, 1 };
        recipe = new Recipe("SilverBar", items, amounts, 3f);
        recipes.Add(recipe);

        recipeDic.Add("Constructor", recipes);
    }

    public List<Recipe> GetRecipeInven(string str)
    {
        recipes = recipeDic[str];
        return recipes;
    }
}
