using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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
