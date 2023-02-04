using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public List<GameObject> items = new List<GameObject>();
    public Inventory inventory; // 플레이어 인벤토리(GameManager)

    private void Update()
    {
        if (Input.GetButton("Loot"))
            Loot();

        // 인벤토리가 열려있을 때 마우스 클릭 처리
        // 일단 여기 넣었지만 화면 클릭을 통한 무언가를 다른 스크립트에서 처리한다면 그쪽으로 이동
        if (EventSystem.current.IsPointerOverGameObject())
            return;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if(itemProps)
            items.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if (itemProps && items.Contains(collision.gameObject))
            items.Remove(collision.gameObject);
    }

    private void Loot()
    {
        foreach (GameObject item in items)
        {
            ItemProps itemProps = item.GetComponent<ItemProps>();
            if (itemProps)
            {
                // 인벤토리에 넣음
                bool wasPickedUp = inventory.Add(itemProps.item, itemProps.amount, true);
                if (wasPickedUp)
                {
                    items.Remove(item);
                    Destroy(item);
                    break;
                }
            }
        }
    }
}
