using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoWindow : MonoBehaviour
{
    [SerializeField]
    GameObject obj;
    [SerializeField]
    GameObject image;
    [SerializeField]
    Text winText;

    bool IsOpen;
    Vector3 mousePos;
    float popupWidth;
    float popupHeight;

    private void Update()
    {
        if (IsOpen)
        {
            mousePos = Input.mousePosition;
            popupWidth = image.GetComponent<RectTransform>().rect.width;
            popupHeight = image.GetComponent<RectTransform>().rect.height;
            Vector2 newPos = new Vector2(mousePos.x + popupWidth / 2, mousePos.y - popupHeight / 2);

            obj.transform.position = newPos;
        }
    }

    public void OpenWindow(Slot slot)
    {
        if(slot.item != null)
        {
            image.SetActive(true);
            IsOpen = true;
            winText.text = slot.item.name;
        }
    }

    public void CloseWindow()
    {
        image.SetActive(false);
        IsOpen = false;
    }
}
