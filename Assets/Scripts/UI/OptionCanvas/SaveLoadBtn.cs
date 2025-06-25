using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadBtn : MonoBehaviour
{
    [SerializeField]
    Text slotText;
    [SerializeField]
    Text contentsText;
    [SerializeField]
    Button mainBtn;
    [SerializeField]
    Button deleteBtn;
    SaveLoadMenu saveLoadMenu;
    bool saveLoadState; // true : save 버튼, false : load 버튼
    int slotNum;
    bool loadEnable;
    string saveFileName;
    SoundManager soundManager;

    private void Start()
    {
        soundManager = SoundManager.instance;
        saveLoadMenu = SaveLoadMenu.instance;
        mainBtn.onClick.AddListener(() => BtnFunc());
    }

    public void SetSlotData(int slotCount, string saveDate, string fileName, int mapDataIndex, int diffLevel)
    {
        slotText.text = "Slot " + slotCount;
        string mapSizeString = MapSizeString(mapDataIndex);
        string diffLevelString = DiffLevelString(diffLevel);
        if (saveDate != null && fileName != null)
        {
            //contentsText.text = saveDate + System.Environment.NewLine + mapSizeString + " " + diffLevelString + " " + fileName;
            contentsText.text = saveDate + System.Environment.NewLine + fileName;
            saveFileName = fileName;
            loadEnable = true;

            deleteBtn.gameObject.SetActive(true);
            deleteBtn.onClick.AddListener(() => DeleteBtnFunc());
        }
        else
        {
            contentsText.text = "Empty";
            saveFileName = "";
            loadEnable = false;

            deleteBtn.gameObject.SetActive(false);
            deleteBtn.onClick.RemoveAllListeners();
        }

        slotNum = slotCount;
    }

    public void SetSlotData(int slotCount, string saveDate) // 자동 저장용
    {
        slotText.text = "Auto";
        if (saveDate != null)
        {
            contentsText.text = saveDate;
            loadEnable = true;
        }
        else
        {
            contentsText.text = "Auto Save Empty";
            loadEnable = false;
        }

        slotNum = slotCount;
    }

    public void RemoveSlotData()
    {
        contentsText.text = "Empty";
        loadEnable = false;

        deleteBtn.gameObject.SetActive(false);
        deleteBtn.onClick.RemoveAllListeners();
    }

    string MapSizeString(int mapDataIndex)
    {
        string mapSize = "";

        switch(mapDataIndex) 
        {
            case 0 :
                mapSize = "Map Small";
                break;
            case 1:
                mapSize = "Map Middle";
                break;
            case 2:
                mapSize = "Map Big";
                break;

            default:
                mapSize = "";
                break;
        }

        return mapSize;
    }

    string DiffLevelString(int mapDataIndex)
    {
        string DiffLevel = "";

        switch (mapDataIndex)
        {
            case 0:
                DiffLevel = "Level Easy";
                break;
            case 1:
                DiffLevel = "Level Normal";
                break;
            case 2:
                DiffLevel = "Level Hard";
                break;

            default:
                DiffLevel = "";
                break;
        }

        return DiffLevel;
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
            ConfirmPanel.instance.CallConfirm(this, saveLoadState, slotNum, saveFileName);
        }
        soundManager.PlayUISFX("ButtonClick");
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
                saveLoadMenu.LoadConfirm(slotNum);
                //saveLoadMenu.Load(slotNum);
            }
            saveLoadMenu.MenuClose();
            OptionCanvas.instance.MainPanelSet(false);
        }
    }

    void DeleteBtnFunc()
    {
        ConfirmPanel.instance.CallDeleteConfirm(this, slotNum);
    }

    public void DeleteBtnConfirm(bool confirmState)
    {
        if (confirmState)
        {
            saveLoadMenu.Delete(slotNum);
            RemoveSlotData();
        }
    }
}
