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
    [SerializeField] Button AllBtn;
    [SerializeField] Button SystemBtn;
    [SerializeField] Button StrBtn;
    [SerializeField] Button UnitBtn;
    [SerializeField] Button ItemBtn;
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
        AllBtn.onClick.AddListener(() => SetType(-1));
        SystemBtn.onClick.AddListener(() => SetType(0));
        StrBtn.onClick.AddListener(() => SetType(1));
        UnitBtn.onClick.AddListener(() => SetType(2));
        ItemBtn.onClick.AddListener(() => SetType(3));
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

    public void SetType(int type)
    {
        sortType = type;
        Search(inputField.text);
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
        ClearRenderTexture();
        InputManager.instance.CloseInfoDic();

        isOpen = false;
        InfoDicObj.SetActive(false);
        SoundManager.instance.PlayUISFX("CloseUI");
        GameManager.instance.onUIChangedCallback?.Invoke(InfoDicObj);
    }
}
