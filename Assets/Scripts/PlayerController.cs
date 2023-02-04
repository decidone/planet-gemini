using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public List<GameObject> items = new List<GameObject>();
    public Inventory inventory; // �÷��̾� �κ��丮(GameManager)

    private void Update()
    {
        if (Input.GetButton("Loot"))
            Loot();

        // �κ��丮�� �������� �� ���콺 Ŭ�� ó��
        // �ϴ� ���� �־����� ȭ�� Ŭ���� ���� ���𰡸� �ٸ� ��ũ��Ʈ���� ó���Ѵٸ� �������� �̵�
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
                // �κ��丮�� ����
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
