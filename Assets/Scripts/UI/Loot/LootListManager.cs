using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootListManager : MonoBehaviour
{
    public GameObject lootItemPrefab;
    public GameObject lootListContent;
    public ScrollRect scrollRect;
    public float defaultYPos;

    List<GameObject> lootInfoList = new List<GameObject>();
    List<Item> lootList = new List<Item>();

    List<GameObject> dropInfoList = new List<GameObject>();
    List<Item> dropList = new List<Item>();

    bool isDropMessageDisplayed;

    #region Singleton
    public static LootListManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void InfoWindowPosChange(int zoomLevel)
    {
        scrollRect.transform.localPosition
            = new Vector3(scrollRect.transform.localPosition.x, defaultYPos + (50 * (zoomLevel - 1)), scrollRect.transform.localPosition.z);
    }

    public void DisplayLootInfo(Item item, int amount)
    {
        if (!lootList.Contains(item))
        {
            GameObject createdItem = Instantiate(lootItemPrefab);
            createdItem.GetComponent<LootItemUI>().SetLootData(item, amount);
            createdItem.transform.SetParent(lootListContent.transform);
            createdItem.transform.localScale = Vector3.one;
            SetScrollToBottom();

            lootList.Add(item);
            lootInfoList.Add(createdItem);
        }
        else
        {
            foreach (GameObject obj in lootInfoList)
            {
                LootItemUI lootUI = obj.GetComponent<LootItemUI>();
                if (lootUI.item == item)
                {
                    lootUI.SetLootData(item, lootUI.amount + amount);
                }
            }
        }
    }

    public void DisplayDropInfo(Item item, int amount)
{
        if (!dropList.Contains(item))
        {
            GameObject createdItem = Instantiate(lootItemPrefab);
            createdItem.GetComponent<LootItemUI>().SetDropMessage(item, amount);
            createdItem.transform.SetParent(lootListContent.transform);
            createdItem.transform.localScale = Vector3.one;
            SetScrollToBottom();

            dropList.Add(item);
            dropInfoList.Add(createdItem);
        }
        else
        {
            foreach (GameObject obj in dropInfoList)
            {
                LootItemUI lootUI = obj.GetComponent<LootItemUI>();
                if (lootUI.item == item)
                {
                    lootUI.SetDropMessage(item, lootUI.amount + amount);
                }
            }
        }
    }

    public void DisplayInfoMessage(string message)
    {
        // 마켓에서 아이템을 버리려고 할 때 뜨는 메시지
        if (isDropMessageDisplayed)
            return;

        GameObject createdItem = Instantiate(lootItemPrefab);
        createdItem.GetComponent<LootItemUI>().SetMessage(message);
        createdItem.transform.SetParent(lootListContent.transform);
        createdItem.transform.localScale = Vector3.one;
        SetScrollToBottom();
        isDropMessageDisplayed = true;
    }

    void SetScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void RemoveListItem(GameObject obj)
    {
        LootItemUI lootUI = obj.GetComponent<LootItemUI>();
        if (lootUI.isMessage)
        {
            isDropMessageDisplayed = false;
            Destroy(obj);
            SetScrollToBottom();
        }
        else
        {
            Item item = lootUI.item;
            if (lootInfoList.Contains(obj))
            {
                lootList.Remove(item);
                lootInfoList.Remove(obj);
                Destroy(obj);
                SetScrollToBottom();
            }
            else if (dropInfoList.Contains(obj))
            {
                dropList.Remove(item);
                dropInfoList.Remove(obj);
                Destroy(obj);
                SetScrollToBottom();
            }
        }
    }
}
