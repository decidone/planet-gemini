using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadBtn : MonoBehaviour
{
    [SerializeField]
    Text slotText;
    [SerializeField]
    Text contentsText;
    SaveLoadMenu saveLoadMenu;
    bool saveLoadState; // true : save 버튼, false : load 버튼
    int slotNum;
    bool loadEnable;

    private void Start()
    {
        saveLoadMenu = SaveLoadMenu.instance;
        GetComponent<Button>().onClick.AddListener(() => BtnFunc());
    }

    public void SetSlotData(int slotCount, string saveDate, string fileName)
    {
        slotText.text = "Slot " + slotCount;
        if (saveDate != null && fileName != null)
        {
            contentsText.text = saveDate + System.Environment.NewLine + fileName;
            loadEnable = true;
        }
        else
        {
            contentsText.text = "Empty";
            loadEnable = false;
        }

        slotNum = slotCount;
    }

    public void SetSlotData(int slotCount, string saveDate) // 자동 저장용
    {
        slotText.text = "Auto";
        if (saveDate != null)
        {
            contentsText.text = "Auto Save : " + saveDate;
            loadEnable = true;
        }
        else
        {
            contentsText.text = "Auto Save Empty";
            loadEnable = false;
        }

        slotNum = slotCount;
    }

    public void BtnStateSet(bool state)
    {
        saveLoadState = state;
    }

    void BtnFunc()
    {
        if (!loadEnable && !saveLoadState)
        {
            return;
        }
        else
        {
            ConfirmPanel.instance.CallConfirm(this, saveLoadState, slotNum);
        }
    }

    public void BtnConfirm(bool confirmState, string fileName)
    {
        if (confirmState)
        {
            if (saveLoadState) // 저장
            {
                saveLoadMenu.Save(slotNum, fileName);
            }
            else // 로드
            {
                saveLoadMenu.Load(slotNum);
            }
        }
    }
}
