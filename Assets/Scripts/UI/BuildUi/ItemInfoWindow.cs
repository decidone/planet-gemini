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

            float clampedX = Mathf.Clamp(newPos.x, popupWidth / 2, Screen.width - popupWidth / 2);
            float clampedY = Mathf.Clamp(newPos.y, popupHeight / 2, Screen.height - popupHeight / 2);

            obj.transform.position = new Vector2(clampedX, clampedY);
        }
    }

    public void OpenWindow(Slot slot)
    {

        if (slot.item != null)
        {
            image.SetActive(true);
            IsOpen = true;
            winText.text = slot.inGameName;
        }
    }

    public void OpenWindow(PortalUIBtn portalUIBtn)
    {
        image.SetActive(true);
        IsOpen = true;
        winText.text = portalUIBtn.inGameName;
    }

    public void CloseWindow()
    {
        image.SetActive(false);
        IsOpen = false;
    }
}
