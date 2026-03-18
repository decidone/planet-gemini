using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InfoDictionary : MonoBehaviour
{
    [SerializeField] InfoDictionaryListSO infoDictionaryListSO;
    [SerializeField] GameObject InfoDicObj;
    [SerializeField] GameObject listItemPref;
    [SerializeField] GameObject content;
    [SerializeField] InputField inputField;
    Dictionary<string, List<Recipe>> recipeDic = new Dictionary<string, List<Recipe>>();

    [Space]
    List<Button> buttonList = new List<Button>();
    [SerializeField] Button allBtn;
    [SerializeField] Button systemBtn;
    [SerializeField] Button strBtn;
    [SerializeField] Button unitBtn;
    [SerializeField] Button itemBtn;
    bool allBtnClicked;
    bool systemBtnClicked;
    bool strBtnClicked;
    bool unitBtnClicked;
    bool itemBtnClicked;
    int sortType = -1;   // -1: 전체, 0: 시스템, 1: 건물, 2: 유닛, 3: 아이템

    [Space]
    [SerializeField] GameObject layout1;
    [SerializeField] Text layout1Name;
    [SerializeField] Text layout1Explanation;
    [SerializeField] VideoPlayer layout1VideoPlayer;

    [Space]
    [SerializeField] GameObject layout2;
    [SerializeField] Text layout2Name;
    [SerializeField] Text layout2Explanation;
    [SerializeField] GameObject layout2ProdRatioObj;
    [SerializeField] Text layout2ProdRatio;
    [SerializeField] Image itemIcon;
    [SerializeField] GameObject market;
    [SerializeField] GameObject tree;
    [SerializeField] GameObject spawner;
    [SerializeField] GameObject structure;
    [SerializeField] Image structureImage;
    [SerializeField] Text structureText;

    [SerializeField] GameObject recipe1;
    [SerializeField] InfoDictionaryIcon recipe1Prod;
    [SerializeField] InfoDictionaryIcon recipe1Mat;
    [SerializeField] InfoDictionaryIcon recipe1Time;
    [SerializeField] Button recipe1Btn;
    [SerializeField] GameObject recipe1Sub;
    [SerializeField] InfoDictionaryIcon recipe1SubProd;
    [SerializeField] InfoDictionaryIcon recipe1SubMat;
    [SerializeField] InfoDictionaryIcon recipe1SubTime;

    [SerializeField] GameObject recipe2;
    [SerializeField] InfoDictionaryIcon recipe2Prod;
    [SerializeField] InfoDictionaryIcon recipe2Mat1;
    [SerializeField] InfoDictionaryIcon recipe2Mat2;
    [SerializeField] InfoDictionaryIcon recipe2Time;
    [SerializeField] Button recipe2Btn;

    [SerializeField] GameObject recipe3;
    [SerializeField] InfoDictionaryIcon recipe3Prod;
    [SerializeField] InfoDictionaryIcon recipe3Mat1;
    [SerializeField] InfoDictionaryIcon recipe3Mat2;
    [SerializeField] InfoDictionaryIcon recipe3Mat3;
    [SerializeField] InfoDictionaryIcon recipe3Time;
    [SerializeField] Button recipe3Btn;

    [Space]
    [HideInInspector] public bool isOpen;
    InfoDictionarySO selectedItem;
    List<GameObject> objList = new List<GameObject>();
    List<InfoDictionarySO> dicList = new List<InfoDictionarySO>();

    #region Singleton
    public static InfoDictionary instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        CreateListItem();
        inputField.onValueChanged.AddListener(Search);

        allBtnClicked = false;
        systemBtnClicked = false;
        strBtnClicked = false;
        unitBtnClicked = false;
        itemBtnClicked = false;

        buttonList.Add(allBtn);
        buttonList.Add(systemBtn);
        buttonList.Add(strBtn);
        buttonList.Add(unitBtn);
        buttonList.Add(itemBtn);

        allBtn.onClick.AddListener(() => BtnClicked(allBtn, -1));
        systemBtn.onClick.AddListener(() => BtnClicked(systemBtn, 0));
        strBtn.onClick.AddListener(() => BtnClicked(strBtn, 1));
        unitBtn.onClick.AddListener(() => BtnClicked(unitBtn, 2));
        itemBtn.onClick.AddListener(() => BtnClicked(itemBtn, 3));
        recipe1Btn.onClick.AddListener(() => ToggleRatio());
        recipe2Btn.onClick.AddListener(() => ToggleRatio());
        recipe3Btn.onClick.AddListener(() => ToggleRatio());
    }

    void BtnClicked(Button btn, int btnNum)
    {
        // -1: 전체, 0: 시스템, 1: 건물, 2: 유닛, 3: 아이템
        switch (btnNum)
        {
            case -1:
                allBtnClicked = true;
                systemBtnClicked = false;
                strBtnClicked = false;
                unitBtnClicked = false;
                itemBtnClicked = false;
                BtnToggle(btn, allBtnClicked);
                SetType(btnNum);
                break;
            case 0:
                systemBtnClicked = !systemBtnClicked;
                allBtnClicked = false;
                strBtnClicked = false;
                unitBtnClicked = false;
                itemBtnClicked = false;
                BtnToggle(btn, systemBtnClicked);
                if (systemBtnClicked)
                    SetType(btnNum);
                else
                    SetType(-1);
                break;
            case 1:
                strBtnClicked = !strBtnClicked;
                allBtnClicked = false;
                systemBtnClicked = false;
                unitBtnClicked = false;
                itemBtnClicked = false;
                BtnToggle(btn, strBtnClicked);
                if (strBtnClicked)
                    SetType(btnNum);
                else
                    SetType(-1);
                break;
            case 2:
                unitBtnClicked = !unitBtnClicked;
                allBtnClicked = false;
                systemBtnClicked = false;
                strBtnClicked = false;
                itemBtnClicked = false;
                BtnToggle(btn, unitBtnClicked);
                if (unitBtnClicked)
                    SetType(btnNum);
                else
                    SetType(-1);
                break;
            case 3:
                itemBtnClicked = !itemBtnClicked;
                allBtnClicked = false;
                systemBtnClicked = false;
                strBtnClicked = false;
                unitBtnClicked = false;
                BtnToggle(btn, itemBtnClicked);
                if (itemBtnClicked)
                    SetType(btnNum);
                else
                    SetType(-1);
                break;
        }
    }

    void ResetButtons(bool isAllOn)
    {
        foreach (Button btn in buttonList)
        {
            Image image = btn.GetComponent<Image>();
            if (isAllOn && btn == allBtn)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
                continue;
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);
            btn.onClick.AddListener(() => SoundManager.instance.PlayUISFX("ButtonClick"));
        }
    }

    void BtnToggle(Button btn, bool isOn)
    {
        Image image = btn.GetComponent<Image>();
        if (isOn)
        {
            ResetButtons(false);
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        }
        else
        {
            ResetButtons(true);
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);
        }
    }

    public void SetType(int type)
    {
        sortType = type;
        Search(inputField.text);
    }

    void CreateListItem()
    {
        BuildingListSO buildingListSO = (BuildingListSO)Resources.Load("SOList/BuildingListSO");
        string json = Resources.Load<TextAsset>("Recipe").ToString();
        recipeDic = JsonConvert.DeserializeObject<Dictionary<string, List<Recipe>>>(json);
        dicList = infoDictionaryListSO.infoDictionarySOList;

        foreach (InfoDictionarySO item in dicList)
        {
            item.recipes = new List<Recipe>();
            foreach (var recipeList in recipeDic)
            {
                if (recipeList.Key.Equals("Tower"))  // 일반적인 생산 레시피는 순서 상 타워 전까지만 있음
                    break;

                foreach (var recipe in recipeList.Value)
                {
                    if (recipe.name.Equals(item.name))
                    {
                        //item.layout = 2;
                        item.recipes.Add(recipe);
                        for (int i = 0; i < buildingListSO.buildingSOList.Count; i++)
                        {
                            // 2개 이상의 레시피를 가지는 아이템은 있지만 2개 이상의 생산건물을 가지는 아이템은 없어서 그냥 마지막 레시피의 생산건물로 지정함
                            if (buildingListSO.buildingSOList[i].scienceName.Equals(recipeList.Key))
                            {
                                item.productionBuilding = buildingListSO.buildingSOList[i];
                            }
                        }
                    }
                }
            }

            GameObject createdItem = Instantiate(listItemPref);
            createdItem.GetComponent<InfoDictionaryItem>().SetData(item);
            createdItem.transform.SetParent(content.transform);
            createdItem.transform.localScale = Vector3.one;

            objList.Add(createdItem);
        }
    }

    public void SelectItem(InfoDictionarySO info)
    {
        selectedItem = info;
        layout1.SetActive(false);
        layout2.SetActive(false);

        if (info.layout == 1)
        {
            layout1.SetActive(true);
            if (info.type <= 1)
            {
                // 시스템, 건물
                layout1Name.text = info.name;
            }
            else
            {
                // 유닛, 아이템
                string inGameName = InGameNameDataGet.instance.ReturnName(info.name);
                layout1Name.text = (inGameName != "") ? inGameName : info.name;
            }
            layout1Explanation.text = info.explanation;
            if (info.videoClip != null)
            {
                layout1VideoPlayer.clip = info.videoClip;
            }
            else
            {
                ClearRenderTexture();
            }
        }
        else if (info.layout == 2)
        {
            ClearRenderTexture();
            layout2.SetActive(true);
            market.SetActive(false);
            tree.SetActive(false);
            spawner.SetActive(false);
            structure.SetActive(false);
            recipe1.SetActive(false);
            recipe1Sub.SetActive(false);
            recipe2.SetActive(false);
            recipe3.SetActive(false);
            layout2ProdRatioObj.SetActive(false);
            layout2ProdRatio.text = "";
            layout2ProdRatio.fontSize = 18;
            recipe1Btn.gameObject.SetActive(false);
            recipe2Btn.gameObject.SetActive(false);
            recipe3Btn.gameObject.SetActive(false);

            if (info.type <= 1)
            {
                // 시스템, 건물
                layout2Name.text = info.name;
            }
            else
            {
                // 유닛, 아이템
                string inGameName = InGameNameDataGet.instance.ReturnName(info.name);
                layout2Name.text = (inGameName != "") ? inGameName : info.name;
            }
            layout2Explanation.text = info.explanation;
            Item item = ItemList.instance.itemDic[info.name];

            if (info.canBuy)
                market.SetActive(true);

            if (item != null)
            {
                itemIcon.sprite = item.icon;

                if (item.name == "VoidShard")
                {
                    spawner.SetActive(true);
                }
                else if (item.name == "Log")
                {
                    tree.SetActive(true);
                }
                else if (item.name == "Water" || item.name == "CrudeOil")
                {
                    structure.SetActive(true);
                    structureImage.sprite = info.productionBuilding.item.icon;
                    structureText.text = info.productionBuilding.scienceName;
                }

                // 채굴기에서 생산하는 아이템
                if (item.tier == 0 && item.name != "UICancel")
                {
                    structure.SetActive(true);
                    structureImage.sprite = info.productionBuilding.item.icon;
                    structureText.text = info.productionBuilding.scienceName;
                }
            }

            if (info.recipes.Count == 1)
            {
                structure.SetActive(true);
                structureImage.sprite = info.productionBuilding.item.icon;
                string inGameName = InGameNameDataGet.instance.ReturnName(info.productionBuilding.scienceName);
                structureText.text = (inGameName != "") ? inGameName : info.productionBuilding.scienceName;
                Recipe recipe = info.recipes[0];
                string ratio = GetRatioText(recipe);
                layout2ProdRatio.text = ratio;
                if (ratio.Length > 50)
                    layout2ProdRatio.fontSize = 16;

                if (recipe.items.Count == 2)
                {
                    recipe1.SetActive(true);
                    recipe1Mat.SetIcon(ItemList.instance.itemDic[recipe.items[0]].icon, recipe.items[0], recipe.amounts[0].ToString(), true);
                    recipe1Prod.SetIcon(ItemList.instance.itemDic[recipe.items[1]].icon, recipe.items[1], recipe.amounts[1].ToString(), false);
                    recipe1Time.SetIcon(recipe.cooldown.ToString());
                    if (ratio != "")
                        recipe1Btn.gameObject.SetActive(true);
                }
                else if (recipe.items.Count == 3)
                {
                    recipe2.SetActive(true);
                    recipe2Mat1.SetIcon(ItemList.instance.itemDic[recipe.items[0]].icon, recipe.items[0], recipe.amounts[0].ToString(), true);
                    recipe2Mat2.SetIcon(ItemList.instance.itemDic[recipe.items[1]].icon, recipe.items[1], recipe.amounts[1].ToString(), true);
                    recipe2Prod.SetIcon(ItemList.instance.itemDic[recipe.items[2]].icon, recipe.items[2], recipe.amounts[2].ToString(), false);
                    recipe2Time.SetIcon(recipe.cooldown.ToString());
                    if (ratio != "")
                        recipe2Btn.gameObject.SetActive(true);
                }
                else if (recipe.items.Count == 4)
                {
                    recipe3.SetActive(true);
                    recipe3Mat1.SetIcon(ItemList.instance.itemDic[recipe.items[0]].icon, recipe.items[0], recipe.amounts[0].ToString(), true);
                    recipe3Mat2.SetIcon(ItemList.instance.itemDic[recipe.items[1]].icon, recipe.items[1], recipe.amounts[1].ToString(), true);
                    recipe3Mat3.SetIcon(ItemList.instance.itemDic[recipe.items[2]].icon, recipe.items[2], recipe.amounts[2].ToString(), true);
                    recipe3Prod.SetIcon(ItemList.instance.itemDic[recipe.items[3]].icon, recipe.items[3], recipe.amounts[3].ToString(), false);
                    recipe3Time.SetIcon(recipe.cooldown.ToString());
                    if (ratio != "")
                        recipe3Btn.gameObject.SetActive(true);
                }
            }
            else if (info.recipes.Count == 2)
            {
                // 플라스틱, 고무만 해당
                structure.SetActive(true);
                structureImage.sprite = info.productionBuilding.item.icon;
                string inGameName = InGameNameDataGet.instance.ReturnName(info.productionBuilding.scienceName);
                structureText.text = (inGameName != "") ? inGameName : info.productionBuilding.scienceName;
                string ratio = GetRatioText(info.recipes[0]);
                layout2ProdRatio.text = ratio;

                recipe1.SetActive(true);
                recipe1Mat.SetIcon(ItemList.instance.itemDic[info.recipes[0].items[0]].icon, info.recipes[0].items[0], info.recipes[0].amounts[0].ToString(), true);
                recipe1Prod.SetIcon(ItemList.instance.itemDic[info.recipes[0].items[1]].icon, info.recipes[0].items[1], info.recipes[0].amounts[1].ToString(), false);
                recipe1Time.SetIcon(info.recipes[0].cooldown.ToString());
                if (ratio != "")
                    recipe1Btn.gameObject.SetActive(true);

                recipe1Sub.SetActive(true);
                recipe1SubMat.SetIcon(ItemList.instance.itemDic[info.recipes[1].items[0]].icon, info.recipes[1].items[0], info.recipes[1].amounts[0].ToString(), true);
                recipe1SubProd.SetIcon(ItemList.instance.itemDic[info.recipes[1].items[1]].icon, info.recipes[1].items[1], info.recipes[1].amounts[1].ToString(), false);
                recipe1SubTime.SetIcon(info.recipes[1].cooldown.ToString());
            }
        }
    }

    void ToggleRatio()
    {
        if (layout2ProdRatioObj.activeSelf)
        {
            layout2ProdRatioObj.SetActive(false);
        }
        else
        {
            layout2ProdRatioObj.SetActive(true);
        }
    }

    void ClearRenderTexture()
    {
        layout1VideoPlayer.clip = null;
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = layout1VideoPlayer.targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    public void Search(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            ResetList();
            if (sortType != -1)
            {
                foreach (GameObject obj in objList)
                {
                    InfoDictionaryItem item = obj.GetComponent<InfoDictionaryItem>();
                    if (item.infoDictionarySO.type != sortType)
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }
        else
        {
            ResetList();
            foreach (GameObject obj in objList)
            {
                InfoDictionaryItem item = obj.GetComponent<InfoDictionaryItem>();
                if (!item.itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || (sortType != -1 && item.infoDictionarySO.type != sortType))
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    public void Search(string keyword, bool isTargetSearch)
    {
        if (isTargetSearch)     //키워드로 바로 인포를 띄워줄 때
        {
            foreach (InfoDictionarySO infoSO in dicList)
            {
                if (infoSO.name.Equals(keyword))
                {
                    OpenUI();
                    SelectItem(infoSO);
                    break;
                }
            }
        }
    }

    public void Search(Structure str)
    {
        //Debug.Log("str.name " + str.name);
        //Debug.Log("InGameNameDataGet.instance.ReturnName(str.name) " + InGameNameDataGet.instance.ReturnName(str.level + 1, str.buildName));

        foreach (InfoDictionarySO infoSO in dicList)
        {
            if (infoSO.strDataList.Contains(str.structureData))
            {
                OpenUI();
                SelectItem(infoSO);
                break;
            }
        }
    }

    public void ResetList()
    {
        foreach (GameObject obj in objList)
        {
            obj.SetActive(true);
        }
    }

    string GetRatioText(Recipe recipe)
    {
        if (recipe == null || recipe.cooldown <= 0) return "";

        List<string> ratios = new List<string>();

        for (int i = 0; i < recipe.items.Count - 1; i++)
        {
            string mat = recipe.items[i];
            int matAmount = recipe.amounts[i];

            Recipe matRecipe = null;
            foreach (var kvp in recipeDic)
            {
                foreach (var r in kvp.Value)
                {
                    if (r.name == mat && r.cooldown > 0)
                    {
                        matRecipe = r;
                        break;
                    }
                }
                if (matRecipe != null) break;
            }

            if (matRecipe != null)
            {
                int matProduct = matRecipe.amounts[matRecipe.amounts.Count - 1];
                float ratio = (matAmount * matRecipe.cooldown) / (float)(matProduct * recipe.cooldown);
                ratios.Add($"{InGameNameDataGet.instance.ReturnName(mat)}({SimplifyRatio(ratio)})");
            }
        }

        return string.Join(", ", ratios);
    }

    string SimplifyRatio(float ratio)   // 생산공장 비율을 정수비로 바꿔줌
    {
        for (int d = 1; d <= 10; d++)
        {
            float n = ratio * d;
            if (Mathf.Abs(n - Mathf.Round(n)) < 0.01f)
            {
                int num = Mathf.RoundToInt(n);
                int gcd = GCD(num, d);
                return $"{num / gcd}:{d / gcd}";
            }
        }
        return $"{ratio:F2}:1";
    }

    int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

    public void OpenUI()
    {
        if (selectedItem != null)
            SelectItem(selectedItem);
        InputManager.instance.OpenInfoDic();

        isOpen = true;
        InfoDicObj.SetActive(true);
        SoundManager.instance.PlayUISFX("SidebarClick");
        GameManager.instance.onUIChangedCallback?.Invoke(InfoDicObj);
    }

    public void CloseUI()
    {
        sortType = -1;
        inputField.text = string.Empty;
        ResetList();
        BtnClicked(allBtn, -1);
        ClearRenderTexture();
        InputManager.instance.CloseInfoDic();

        isOpen = false;
        InfoDicObj.SetActive(false);
        SoundManager.instance.PlayUISFX("CloseUI");
        GameManager.instance.onUIChangedCallback?.Invoke(InfoDicObj);
    }
}
