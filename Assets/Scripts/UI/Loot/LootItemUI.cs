using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootItemUI : MonoBehaviour
{
    [SerializeField] Text lootInfoText;
    public bool isMessage;
    public Item item;
    public int amount;
    public float timer;
    public bool isTimerOn = true;

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        if (isTimerOn && timer > 5f)
        {
            DestroyUI();
            isTimerOn = false;
        }
    }

    public void SetLootData(Item _item, int _amount)
    {
        isMessage = false;
        timer = 0;
        item = _item;
        amount = _amount;
        lootInfoText.text = InGameNameDataGet.instance.ReturnName(item.name) + ", " + amount;
    }

    public void SetDropMessage(Item _item, int _amount)
    {
        isMessage = false;
        timer = 0;
        item = _item;
        amount = _amount;
        lootInfoText.text = "Drop " + InGameNameDataGet.instance.ReturnName(item.name) + ", " + amount;
        lootInfoText.color = Color.red;
    }

    public void SetMessage(string message)
    {
        isMessage = true;
        timer = 0;
        lootInfoText.text = message;
        lootInfoText.color = Color.red;
    }

    public void DestroyUI()
    {
        LootListManager.instance.RemoveListItem(gameObject);
    }
}
