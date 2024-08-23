using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootListManager : MonoBehaviour
{
    public GameObject lootItemPrefab;
    public GameObject lootListContent;
    public ScrollRect scrollRect;

    List<GameObject> lootInfoList = new List<GameObject>();
    List<Item> lootList = new List<Item>();

    List<GameObject> dropInfoList = new List<GameObject>();
    List<Item> dropList = new List<Item>();

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
            StartCoroutine(DestroyLootInfo(createdItem));
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

    IEnumerator DestroyLootInfo(GameObject obj)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            LootItemUI lootUI = obj.GetComponent<LootItemUI>();
            if (lootUI.timer > 5f)
            {
                lootList.Remove(lootUI.item);
                lootInfoList.Remove(obj);
                Destroy(obj);
                SetScrollToBottom();
                yield break;
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
            StartCoroutine(DestroyDropInfo(createdItem));
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

    IEnumerator DestroyDropInfo(GameObject obj)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            LootItemUI lootUI = obj.GetComponent<LootItemUI>();
            if (lootUI.timer > 5f)
            {
                dropList.Remove(lootUI.item);
                dropInfoList.Remove(obj);
                Destroy(obj);
                SetScrollToBottom();
                yield break;
            }
        }
    }

    void SetScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
