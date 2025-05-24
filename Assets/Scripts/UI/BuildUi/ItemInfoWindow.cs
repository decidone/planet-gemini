using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoWindow : MonoBehaviour
{
    public RectTransform canvasRectTransform; // 캔버스의 RectTransform
    public RectTransform imageRectTransform;  // 팝업 이미지의 RectTransform
    [SerializeField]
    GameObject obj;
    [SerializeField]
    GameObject image;
    [SerializeField]
    Text winText;

    bool IsOpen;

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (IsOpen)
        {
            Vector2 mousePos = Input.mousePosition;

            // 캔버스 공간에서의 마우스 좌표로 변환
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePos, null, out anchoredPos);

            // 팝업 이미지의 크기 가져오기
            float popupWidth = imageRectTransform.rect.width;
            float popupHeight = imageRectTransform.rect.height;

            // 새 좌표 계산 (마우스 위치에 대해 오프셋 적용)
            // 왼쪽 위에 위치하도록 마우스 좌표에서 팝업 크기만큼 뺌
            Vector2 newPos = new Vector2(anchoredPos.x + popupWidth / 2, anchoredPos.y + popupHeight / 2);
            // 팝업이 화면 밖으로 나가지 않도록 클램핑
            float clampedX = Mathf.Clamp(newPos.x, -canvasRectTransform.rect.width / 2 + popupWidth / 2, canvasRectTransform.rect.width / 2 - popupWidth / 2);
            float clampedY = Mathf.Clamp(newPos.y, -canvasRectTransform.rect.height / 2 + popupHeight / 2, canvasRectTransform.rect.height / 2 - popupHeight / 2);

            // 위치 설정
            obj.transform.localPosition = new Vector2(clampedX, clampedY);
        }
    }

    public void OpenWindow(Slot slot)
    {
        if (slot.item != null)
        {
            image.SetActive(true);
            IsOpen = true;

            if (slot.strDataSet)
            {
                Vector2 sizeDelta = winText.rectTransform.sizeDelta;
                sizeDelta.y = 60;
                winText.rectTransform.sizeDelta = sizeDelta;
                if (slot.isEnergyStr)
                {
                    winText.text = slot.inGameName + System.Environment.NewLine + "Energy Produce : " + slot.energyProduction;
                }
                else if (slot.isEnergyUse)
                {
                    winText.text = slot.inGameName + System.Environment.NewLine + "Energy Consume : " + slot.energyConsumption;
                }
            }
            else
            {
                winText.text = slot.inGameName;
                if (slot.item.tier == 5)
                {
                    winText.color = new Color(0, 115, 255);
                }
                else if (slot.item.tier == 4)
                {
                    winText.color = new Color(255, 140, 0);
                }
                else
                {
                    winText.color = Color.white;
                }
            }
        }
    }

    public void OpenWindow(PortalUIBtn portalUIBtn)
    {
        image.SetActive(true);
        IsOpen = true;
        winText.text = portalUIBtn.inGameName;
    }

    public void OpenWindow(string name)
    {
        image.SetActive(true);
        IsOpen = true;
        winText.text = name;
    }

    public void CloseWindow()
    {
        image.SetActive(false);
        winText.color = Color.white;
        IsOpen = false;
        Vector2 sizeDelta = winText.rectTransform.sizeDelta;
        sizeDelta.y = 20;
        winText.rectTransform.sizeDelta = sizeDelta;
    }
}
