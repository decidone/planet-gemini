using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecipeList : MonoBehaviour
{
    Dictionary<string, List<Recipe>> recipeDic;
    List<Recipe> recipes;
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

        string json = File.ReadAllText("Assets/Data/Recipe.json");
        recipeDic = JsonConvert.DeserializeObject<Dictionary<string, List<Recipe>>>(json);
    }
    #endregion

    public List<Recipe> GetRecipeInven(string str)
    {
        recipes = recipeDic[str];
        return recipes;
    }
}
