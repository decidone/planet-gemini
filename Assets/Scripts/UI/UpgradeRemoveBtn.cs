using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRemoveBtn : MonoBehaviour
{
    [SerializeField]
    Button upgradeBtn;
    [SerializeField]
    Button removeBtn;

    bool clickBtn;  // 둘중 하나라도 누르면 true
    bool btnSwitch; // true : 업그레이드, false : 제거

    DragGraphic dragGraphic;

    [SerializeField]
    Sprite[] images;

    // Start is called before the first frame update
    void Start()
    {
        dragGraphic = DragGraphic.instance;
        upgradeBtn.onClick.AddListener(() => UpgradeBtnFunc());
        removeBtn.onClick.AddListener(() => RemoveBtnFunc());
    }

    void UpgradeBtnFunc()
    {
        if (clickBtn && btnSwitch)
        {
            clickBtn = false;
            dragGraphic.BtnFuncReset();
            ReSetColor(upgradeBtn);
        }
        else
        {
            clickBtn = true;
            btnSwitch = true;
            dragGraphic.BtnFunc(true);
            SetColor(upgradeBtn);
            ReSetColor(removeBtn);
        }
    }

    void RemoveBtnFunc()
    {
        if (clickBtn && !btnSwitch)
        {
            clickBtn = false;
            dragGraphic.BtnFuncReset();
            ReSetColor(removeBtn);
        }
        else
        {
            clickBtn = true;
            btnSwitch = false;
            dragGraphic.BtnFunc(false);
            SetColor(removeBtn);
            ReSetColor(upgradeBtn);
        }
    }

    void SetColor(Button button)
    {
        Image img = button.GetComponent<Image>();
        img.sprite = images[1];
    }

    void ReSetColor(Button button)
    {
        Image img = button.GetComponent<Image>();
        img.sprite = images[0];
    }
}
