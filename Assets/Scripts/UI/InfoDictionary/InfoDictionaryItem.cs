using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoDictionaryItem : MonoBehaviour
{
    [SerializeField] Sprite systemBackground;
    [SerializeField] Sprite strBackground;
    [SerializeField] Sprite unitBackground;
    [SerializeField] Sprite itemBackground;
    [SerializeField] Image backgroundImg;
    [SerializeField] Text text;
    [SerializeField] Button btn;

    public InfoDictionarySO infoDictionarySO;
    public string itemName;

    public void SetData(InfoDictionarySO info)
    {
        infoDictionarySO = info;
        switch (info.type)
        {
            case 0: backgroundImg.sprite = systemBackground;
                break;
            case 1: backgroundImg.sprite = strBackground;
                break;
            case 2: backgroundImg.sprite = unitBackground;
                break;
            case 3: backgroundImg.sprite = itemBackground;
                break;
        }

        if (info.type <= 1)
        {
            // 시스템, 건물
            itemName = info.name;
            text.text = itemName;
        }
        else
        {
            // 유닛, 아이템
            string inGameName = InGameNameDataGet.instance.ReturnName(info.name);
            itemName = (inGameName != "") ? inGameName : info.name;
            text.text = itemName;
        }

        btn.onClick.AddListener(() => BtnClicked());
    }

    void BtnClicked()
    {
        InfoDictionary.instance.SelectItem(infoDictionarySO);
    }
}
