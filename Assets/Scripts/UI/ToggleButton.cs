using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    public UnityEvent onToggleOn;

    public bool isOn = false;

    public Sprite[] toggleImg = null;

    public GameObject toggleButtonObj = null;
    Button toggleButton = null;
    Image toggleButtonSprite = null;
    public RectTransform toggleSwObj = null;

    // Start is called before the first frame update
    void Awake()
    {
        toggleButton = toggleButtonObj.GetComponent<Button>();
        toggleButtonSprite = toggleButtonObj.GetComponent<Image>();
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleButtonClick);
    }

    public void OpenSetting(bool isOnOff)
    {
        isOn = isOnOff;

        if (toggleButtonSprite == null) 
            toggleButtonSprite = toggleButtonObj.GetComponent<Image>();

        ButtonSetModle(isOn);
    }

    void ToggleButtonClick()
    {
        isOn = !isOn;

        ButtonSetModle(isOn);

        onToggleOn?.Invoke();
    }

    public void ButtonSetModle(bool On)
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
