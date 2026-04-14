using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrWarningManager : MonoBehaviour
{
    [SerializeField] Button warningListBtn;
    [SerializeField] Text warningText;
    public List<Structure> mainPlanetStructureList = new();
    public List<Structure> subPlanetStructureList = new();
    int count;

    #region Singleton
    public static StrWarningManager instance;

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

    private void Start()
    {
        warningListBtn.onClick.AddListener(() => FocusWarningStr());
        SetButtonAndCount();
    }

    public void SetButtonAndCount()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.isPlayerInMarket)
        {
            warningListBtn.gameObject.SetActive(false);
            warningText.text = string.Empty;
        }
        else if (GameManager.instance.isPlayerInHostMap)
        {
            mainPlanetStructureList.RemoveAll(x => !x);

            if (mainPlanetStructureList.Count > 0)
            {
                warningListBtn.gameObject.SetActive(true);
                warningText.text = mainPlanetStructureList.Count.ToString();
            }
            else
            {
                warningListBtn.gameObject.SetActive(false);
                warningText.text = string.Empty;
            }
        }
        else
        {
            subPlanetStructureList.RemoveAll(x => !x);

            if (subPlanetStructureList.Count > 0)
            {
                warningListBtn.gameObject.SetActive(true);
                warningText.text = subPlanetStructureList.Count.ToString();
            }
            else
            {
                warningListBtn.gameObject.SetActive(false);
                warningText.text = string.Empty;
            }
        }
    }

    public void SetButtonAndCount(int num)
    {
        //callback 연결용 오버로딩. num은 안 씀
        SetButtonAndCount();
    }

    public void AddStrList(Structure str)
    {
        if (str.isInHostMap)
        {
            if (!mainPlanetStructureList.Contains(str))
                mainPlanetStructureList.Add(str);
        }
        else
        {
            if (!subPlanetStructureList.Contains(str))
                subPlanetStructureList.Add(str);
        }

        SetButtonAndCount();
    }

    public void RemoveStrList(Structure str)
    {
        if (str.isInHostMap)
        {
            if (mainPlanetStructureList.Contains(str))
                mainPlanetStructureList.Remove(str);
        }
        else
        {
            if (subPlanetStructureList.Contains(str))
                subPlanetStructureList.Remove(str);
        }

        SetButtonAndCount();
    }

    void FocusWarningStr()
    {
        if (GameManager.instance == null || GameManager.instance.isPlayerInMarket) return;

        SetButtonAndCount();

        if (GameManager.instance.isPlayerInHostMap)
        {
            if (mainPlanetStructureList.Count > 0)
            {
                if (!InputManager.instance.isMapOpened)
                {
                    count = 0;
                    Vector3 pos = mainPlanetStructureList[count].transform.position;
                    MapCameraController.instance.ToggleMap(pos);
                }
                else
                {
                    count = ((count + 1) < mainPlanetStructureList.Count) ? count + 1 : 0;
                    Vector3 pos = mainPlanetStructureList[count].transform.position;
                    MapCameraController.instance.SetCamPos(pos, 4);
                }
            }
        }
        else
        {
            if (subPlanetStructureList.Count > 0)
            {
                if (!InputManager.instance.isMapOpened)
                {
                    count = 0;
                    Vector3 pos = subPlanetStructureList[count].transform.position;
                    MapCameraController.instance.ToggleMap(pos);
                }
                else
                {
                    count = ((count + 1) < subPlanetStructureList.Count) ? count + 1 : 0;
                    Vector3 pos = subPlanetStructureList[count].transform.position;
                    MapCameraController.instance.SetCamPos(pos, 4);
                }
            }
        }
    }
}
