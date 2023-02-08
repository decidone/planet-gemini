using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public List<GameObject> items = new List<GameObject>();
    public Inventory inventory; // �÷��̾� �κ��丮(GameManager)

    private void Start()
    {
        inventory = PlayerInventory.instance;
    }

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
                int containableAmount = inventory.SpaceCheck(itemProps.item);
                if (itemProps.amount <= containableAmount)
                {
                    // �κ��丮�� ����
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
