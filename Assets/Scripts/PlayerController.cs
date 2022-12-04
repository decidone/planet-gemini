using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private bool lootEnabled = false;

    private void Update()
    {
        if (Input.GetButtonDown("Loot"))
            lootEnabled = true;
        if (Input.GetButtonUp("Loot"))
            lootEnabled = false;

        // �κ��丮�� �������� �� ���콺 Ŭ�� ó��
        // �ϴ� ���� �־����� ȭ�� Ŭ���� ���� ���𰡸� �ٸ� ��ũ��Ʈ���� ó���Ѵٸ� �������� �̵�
        if (EventSystem.current.IsPointerOverGameObject())
            return;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (lootEnabled)
        {
            ItemProps itemProps = collision.GetComponent<ItemProps>();
            if (itemProps)
            {
                // �κ��丮�� ����
                bool wasPickedUp = Inventory.instance.Add(itemProps.item, itemProps.amount);
                if (wasPickedUp)
                    Destroy(collision.gameObject);
            }
        }
    }
}
