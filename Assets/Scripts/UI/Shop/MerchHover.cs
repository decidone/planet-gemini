using UnityEngine;
using UnityEngine.EventSystems;

public class MerchHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ItemInfoWindow itemInfoWindow;
    string itemName;

    private void Start()
    {
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
    }

    public void SetItemName(string itemName)
    {
        this.itemName = itemName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemInfoWindow.OpenWindow(itemName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemInfoWindow.CloseWindow();
    }
}
