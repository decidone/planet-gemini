using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoDictionaryIcon : MonoBehaviour
{
    [SerializeField] ItemNameTag nameTag;
    [SerializeField] Image icon;
    [SerializeField] Text text;

    public void SetIcon(Sprite sprite, string itemName, string amount)
    {
        icon.sprite = sprite;
        text.text = amount;
        if (nameTag != null)
        {
            nameTag.SetItemName(itemName);
        }
    }
}
