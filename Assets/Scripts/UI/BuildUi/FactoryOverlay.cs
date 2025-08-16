using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryOverlay : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer itemUI;
    [SerializeField]
    SpriteRenderer itemIcon;

    public void UISet(Item item)
    {
        itemUI.enabled = true;
        itemIcon.enabled = true;
        itemIcon.sprite = item.icon;
    }

    public void UIReset()
    {
        itemUI.enabled = false;
        itemIcon.enabled = false;
        itemIcon.sprite = null;
    }
}
