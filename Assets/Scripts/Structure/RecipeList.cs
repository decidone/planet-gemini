using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeList : MonoBehaviour
{
    public Dictionary<string, List<Recipe>> recipeDic;
    public List<Recipe> recipes;
    Dictionary<string, Item> itemDic;

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
        recipeDic = new Dictionary<string, List<Recipe>>();
    }
    #endregion

    void Start()
    {
        itemDic = ItemList.instance.itemDic;

        List<Item> items = new List<Item> { itemDic["Gold"], itemDic["GoldBar"] };
        List<int> amounts = new List<int> { 2, 1 };
        Recipe recipe = new Recipe("GoldBar", 2, items, amounts, 3f);
        recipes.Add(recipe);

        items = new List<Item> { itemDic["Silver"], itemDic["SilverBar"] };
        amounts = new List<int> { 1, 1 };
        recipe = new Recipe("SilverBar", 2, items, amounts, 3f);
        recipes.Add(recipe);

        recipeDic.Add("Constructor", recipes);
    }

    public List<Recipe> GetRecipeInven(string str)
    {
        recipes = recipeDic[str];
        return recipes;
    }
}
