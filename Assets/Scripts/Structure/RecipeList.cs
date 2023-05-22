using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecipeList : MonoBehaviour
{
    Dictionary<string, List<Recipe>> recipeDic;
    List<Recipe> recipes;
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
