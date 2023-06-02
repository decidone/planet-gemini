using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Inventory inventory;

    //�׽�Ʈ��

    public GameObject buildingInfoObj = null;

    //�׽�Ʈ��

    List<GameObject> items = new List<GameObject>();

    void Update()
    {
        if (Input.GetButton("Loot"))
            Loot();

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.F))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            if (hit.collider.TryGetComponent(out SolidFactoryCtrl factoryCtrl))
            {
                List<Item> factItemList = factoryCtrl.PlayerGetItemList();
                for (int i = 0; i < factItemList.Count; i++) 
                {
                    inventory.Add(factItemList[i], 1);
                }
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
                int containableAmount = inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    // �κ��丮�� ����
                    inventory.Add(itemProps.item, itemProps.amount);
                    items.Remove(item);
                    Destroy(item);

                    //�׽�Ʈ��

                    if(buildingInfoObj != null && buildingInfoObj.activeSelf == true)
                        BuildingInfo.instance.SetItemSlot();

                    //�׽�Ʈ��

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
