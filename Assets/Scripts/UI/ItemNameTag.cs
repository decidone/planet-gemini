using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemNameTag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 상점, 사전 페이지 아이템 아이콘에 마우스 올렸을 때 이름 띄워주는 스크립트
    [SerializeField] GameObject mouseHoverTextObj;
    [SerializeField] Text hoverText;
    bool isMouseHover;
    string itemName;

    private void Update()
    {
        if (isMouseHover)
        {
            mouseHoverTextObj.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseHover = true;
        hoverText.text = itemName;
        mouseHoverTextObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseHover = false;
        hoverText.text = string.Empty;
        mouseHoverTextObj.SetActive(false);
    }

    public void SetItemName(string itemName)
    {
        this.itemName = itemName;
    }
}
