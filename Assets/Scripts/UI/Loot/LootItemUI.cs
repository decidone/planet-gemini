using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootItemUI : MonoBehaviour
{
    [SerializeField] Text lootInfoText;
    public Item item;
    public int amount;
    public float timer;

    private void Update()
    {
        timer += Time.deltaTime;
    }

    public void SetLootData(Item _item, int _amount)
    {
        timer = 0;
        item = _item;
        amount = _amount;
        lootInfoText.text = item.name + ", " + amount;
    }

    public void SetDropMessage(Item _item, int _amount)
    {
        timer = 0;
        item = _item;
        amount = _amount;
        lootInfoText.text = "Drop " + item.name + ", " + amount;
        lootInfoText.color = Color.red;
    }
}