using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

// UTF-8 설정
public class ToggleButton : MonoBehaviour
{
    public UnityEvent onToggleOn;
    public bool isOn = true;
    public Sprite[] toggleImg = null;
    public GameObject toggleButtonObj = null;
    public RectTransform toggleSwObj = null;
    Button toggleButton = null;
    Image toggleButtonSprite = null;
    
    void Awake()
    {
        toggleButton = toggleButtonObj.GetComponent<Button>();
        toggleButtonSprite = toggleButtonObj.GetComponent<Image>();
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleButtonClick);
    }

    public void OpenSetting(bool isOnOff)
    {
        if (toggleButtonSprite == null) 
            toggleButtonSprite = toggleButtonObj.GetComponent<Image>();

        ButtonSetModle(isOnOff);
    }

    void ToggleButtonClick()
    {
        isOn = !isOn;

        ButtonSetModle(isOn);

        onToggleOn?.Invoke();
    }

    void ButtonSetModle(bool On)
    {
        if (On)
        {
            toggleButtonSprite.sprite = toggleImg[0];
            Vector3 newPosition = toggleSwObj.anchoredPosition;
            newPosition.x = 15f;
            toggleSwObj.anchoredPosition = newPosition;
        }
        else
        {
            toggleButtonSprite.sprite = toggleImg[1];
            Vector3 newPosition = toggleSwObj.anchoredPosition;
            newPosition.x = -15f;
            toggleSwObj.anchoredPosition = newPosition;
        }

        isOn = On;
    }
}
