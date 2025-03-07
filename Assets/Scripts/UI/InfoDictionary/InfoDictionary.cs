using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InfoDictionary : MonoBehaviour
{
    [SerializeField] InfoDictionaryListSO InfoDictionaryListSO;
    [SerializeField] GameObject InfoDicObj;
    [SerializeField] GameObject listItemPref;
    [SerializeField] GameObject content;
    [SerializeField] InputField inputField;

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
    [SerializeField] Image layout2Image;

    [Space]
    [HideInInspector] public bool isOpen;
    InfoDictionarySO selectedItem;
    List<GameObject> objList = new List<GameObject>();

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

        CreateListItem();
        inputField.onValueChanged.AddListener(Search);
    }
    #endregion

    private void Start()
    {
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
        List<InfoDictionarySO> list = InfoDictionaryListSO.infoDictionarySOList;

        foreach (InfoDictionarySO item in list)
        {
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

        if (info.layout == 1)
        {
            layout1Name.text = info.name;
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
                    || (item.infoDictionarySO.type != -1 && item.infoDictionarySO.type != sortType))
                {
                    obj.SetActive(false);
                }
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

    public void OpenUI()
    {
        if (selectedItem != null)
            SelectItem(selectedItem);
        InputManager.instance.OpenInfoDic();

        isOpen = true;
        InfoDicObj.SetActive(true);
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
