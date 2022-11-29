using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private void Update()
    {
        // 인벤토리가 열려있을 때 마우스 클릭 처리
        // 일단 여기 넣었지만 화면 클릭을 통한 무언가를 다른 스크립트에서 처리한다면 그쪽으로 이동
        if (EventSystem.current.IsPointerOverGameObject())
            return;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        ItemProps itemProps = collision.GetComponent<ItemProps>();
        if (itemProps)
        {
            Debug.Log("inventory : " + itemProps.item.name);
            // 인벤토리에 넣음
            bool wasPickedUp = Inventory.instance.Add(itemProps.item);
            if(wasPickedUp)
                Destroy(collision.gameObject);
        }
    }
}
