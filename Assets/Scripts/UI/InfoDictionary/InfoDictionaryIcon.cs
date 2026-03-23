using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoDictionaryIcon : MonoBehaviour
{
    [SerializeField] ItemNameTag nameTag;
    [SerializeField] Image icon;
    [SerializeField] Text text;
    [SerializeField] Button btn;

    public void SetIcon(Sprite sprite, string itemName, string amount, bool hasLink)
    {
        icon.sprite = sprite;
        text.text = amount;
        if (nameTag != null)
        {
            nameTag.SetItemName(itemName);
        }
        if (hasLink && btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => MoveToLink(itemName));
        }
    }

    void MoveToLink(string item)
    {
        nameTag.SetOff();
        InfoDictionary.instance.Search(item, true);
    }

    public void SetIcon(string amount)
    {
        text.text = amount;
    }
}
