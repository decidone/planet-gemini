using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    List<GameObject> items = new List<GameObject>();
    Inventory inventory;

    void Start()
    {
        inventory = PlayerInventory.instance;
    }

    void Update()
    {
        if (Input.GetButton("Loot"))
            Loot();

        // 인벤토리가 열려있을 때 마우스 클릭 처리
        // 일단 여기 넣었지만 화면 클릭을 통한 무언가를 다른 스크립트에서 처리한다면 그쪽으로 이동
        if (EventSystem.current.IsPointerOverGameObject())
            return;
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
                    // 인벤토리에 넣음
                    inventory.Add(itemProps.item, itemProps.amount);
                    items.Remove(item);
                    Destroy(item);
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
