using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnloaderManager : MonoBehaviour
{
    protected GameManager gameManager;

    public GameObject unloaderUI;
    [SerializeField]
    SplitterFilterRecipe unloaderRecipe;

    Unloader unloader = null;

    public Slot slot;
    
    [SerializeField]
    Button fillterMenuBtn;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;

        slot.amountText.gameObject.SetActive(false);
        fillterMenuBtn.onClick.AddListener(() => UnloaderFillterMenu());
    }
    void UnloaderFillterMenu()
    {
        unloaderRecipe.OpenUI();
        unloaderRecipe.GetFillterNum(0, "UnloaderManager");
    }
    public void SetUnloader(Unloader _unloader)
    {
        unloader = _unloader;
    }

    public void ReleaseInven()
    {
        slot.ResetOption();
        unloader = null;
    }

    public void SetItem(Item _item)
    {
        unloader.selectItem = _item;

        if (slot.item == null)
        {
            slot.AddItem(_item, 1);
        }
        else if (slot.item != _item)
        {
            slot.ClearSlot();
            slot.AddItem(_item, 1);
        }
        else if (slot.item == _item)
            return;
    }

    public void OpenUI()
    {
        if (unloader != null && unloader.selectItem != null)
        {
            slot.AddItem(unloader.selectItem, 1);
        }
        else
            slot.ClearSlot();

        unloaderUI.SetActive(true);

        gameManager.onUIChangedCallback?.Invoke(unloaderUI);
    }

    public void CloseUI()
    {
        unloaderUI.SetActive(false);
        unloaderRecipe.CloseUI();
        gameManager.onUIChangedCallback?.Invoke(unloaderUI);
    }
}
