using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BUIBtn : MonoBehaviour
{
    public bool isStickyKey;
    [SerializeField]
    string stickyKey;
    public string OptionName;
    [SerializeField]
    string keyValue;
    ItemInfoWindow itemInfoWindow;
    [SerializeField]
    bool isSwapBtn;

    private void Start()
    {
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();

        if (isStickyKey)
        {
            keyValue = stickyKey;
        }
    }

    public void KeyValueSet(string key)
    {
        keyValue = key;
    }

    public void OnEnter()
    {
        if(keyValue != "")
            itemInfoWindow.OpenWindow(OptionName + "('" + keyValue + "')");
        else
            itemInfoWindow.OpenWindow(OptionName);
        BasicUIBtns.instance.mouseOnBtn = true;
        BasicUIBtns.instance.isSwapBtn = isSwapBtn;
    }
}
