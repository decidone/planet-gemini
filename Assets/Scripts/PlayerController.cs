using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private void Update()
    {
        // �κ��丮�� �������� �� ���콺 Ŭ�� ó��
        // �ϴ� ���� �־����� ȭ�� Ŭ���� ���� ���𰡸� �ٸ� ��ũ��Ʈ���� ó���Ѵٸ� �������� �̵�
        if (EventSystem.current.IsPointerOverGameObject())
            return;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if (itemProps)
        {
            Debug.Log("inventory : " + itemProps.item.name);
            // �κ��丮�� ����
            bool wasPickedUp = Inventory.instance.Add(itemProps.item);
            if(wasPickedUp)
                Destroy(collision.gameObject);
        }
    }
}
