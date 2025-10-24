using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRemoveBtn : MonoBehaviour
{
    public enum SelectedButton
    {
        None,
        BuildingUpgrade,
        BuildingRemove,
        UnitRemove
    }

    [SerializeField]
    Button buildingUpgradeBtn;
    [SerializeField]
    Button buildingRemoveBtn;
    [SerializeField]
    Button unitRemoveBtn;

    public SelectedButton currentBtn = SelectedButton.None;

    DragGraphic dragGraphic;

    [SerializeField]
    Sprite[] images;
    SoundManager soundManager;
    #region Singleton
    public static UpgradeRemoveBtn instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        soundManager = SoundManager.instance;
        dragGraphic = DragGraphic.instance;
        buildingUpgradeBtn.onClick.AddListener(() => UpgradeBtnFunc());
        buildingRemoveBtn.onClick.AddListener(() => RemoveBtnFunc());
        unitRemoveBtn.onClick.AddListener(() => UnitRemoveBtnFunc());
    }

    void UpgradeBtnFunc()
    {
        if (currentBtn == SelectedButton.BuildingUpgrade)
        {
            // 같은 버튼 다시 눌렀을 때 -> 해제
            currentBtn = SelectedButton.None;
            dragGraphic.BtnFuncReset();
            ReSetColor(buildingUpgradeBtn);
            MouseSkin.instance.ResetCursor();
        }
        else
        {
            currentBtn = SelectedButton.BuildingUpgrade;
            dragGraphic.BtnFunc(currentBtn);
            SetColor(buildingUpgradeBtn);
            ReSetColor(buildingRemoveBtn);
            ReSetColor(unitRemoveBtn);
            MouseSkin.instance.DragCursorSet(false);
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    void RemoveBtnFunc()
    {
        if (currentBtn == SelectedButton.BuildingRemove)
        {
            currentBtn = SelectedButton.None;
            dragGraphic.BtnFuncReset();
            ReSetColor(buildingRemoveBtn);
            MouseSkin.instance.ResetCursor();
        }
        else
        {
            currentBtn = SelectedButton.BuildingRemove;
            dragGraphic.BtnFunc(currentBtn);
            SetColor(buildingRemoveBtn);
            ReSetColor(buildingUpgradeBtn);
            ReSetColor(unitRemoveBtn);
            MouseSkin.instance.DragCursorSet(true);
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    void UnitRemoveBtnFunc()
    {
        if (currentBtn == SelectedButton.UnitRemove)
        {
            currentBtn = SelectedButton.None;
            dragGraphic.BtnFuncReset();
            ReSetColor(unitRemoveBtn);
            MouseSkin.instance.ResetCursor();
        }
        else
        {
            currentBtn = SelectedButton.UnitRemove;
            dragGraphic.BtnFunc(currentBtn);
            SetColor(unitRemoveBtn);
            ReSetColor(buildingUpgradeBtn);
            ReSetColor(buildingRemoveBtn);
            MouseSkin.instance.DragCursorSet(true);
        }
        soundManager.PlayUISFX("ButtonClick");
    }

    public void CurrentBtnReset()
    {
        currentBtn = SelectedButton.None;
        dragGraphic.BtnFuncReset();
        ReSetColor(buildingUpgradeBtn);
        ReSetColor(buildingRemoveBtn);
        ReSetColor(unitRemoveBtn);
        MouseSkin.instance.ResetCursor();
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
