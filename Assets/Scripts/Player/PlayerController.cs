using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Inventory inventory;

    List<GameObject> items = new List<GameObject>();

    void Update()
    {
        if (Input.GetButton("Loot"))
            Loot();

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.collider != null && hit.collider.TryGetComponent(out LogisticsCtrl factoryCtrl))
            {
                List<Item> factItemList = factoryCtrl.PlayerGetItemList();
                for (int i = 0; i < factItemList.Count; i++) 
                {
                    inventory.Add(factItemList[i], 1);
                }
            }
            else if (hit.collider != null && hit.collider.TryGetComponent(out Production production))
            {
                var item = production.QuickPullOut();
                if(item.Item1 != null && item.Item2 > 0)
                    inventory.Add(item.Item1, item.Item2);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if(itemProps)
            items.Add(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);
    }

    void Loot()
    {
        foreach (GameObject item in items)
        {
            ItemProps itemProps = item.GetComponent<ItemProps>();
            if (itemProps)
            {
                if (itemProps.isOnBelt) 
                {
                    itemProps.setOnBelt.PlayerRootItem(itemProps);
                }
                int containableAmount = inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    // 인벤토리에 넣음
                    inventory.Add(itemProps.item, itemProps.amount);
                    items.Remove(item);
                    if (itemProps.isOnBelt)
                    {
                        itemProps.Pool.Release(itemProps.gameObject);
                    }
                    else
                        Destroy(item);

                    if (BuildingInfo.instance != null && BuildingInfo.instance.gameObject.activeSelf)
                        BuildingInfo.instance.SetItemSlot();
                    if (InfoWindow.instance != null && InfoWindow.instance.gameObject.activeSelf)
                    {
                        InfoWindow.instance.SetNeedItem();
                    }

                    break;
                }
                else if (containableAmount != 0)
                {
                    inventory.Add(itemProps.item, containableAmount);
                    itemProps.amount -= containableAmount;
                }
                else
                {
                    Debug.Log("not enough space");
                }
            }
        }
    }
}
