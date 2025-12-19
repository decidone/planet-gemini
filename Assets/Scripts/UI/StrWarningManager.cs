using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrWarningManager : MonoBehaviour
{
    [SerializeField] Button warningListBtn;
    [SerializeField] Text warningText;
    public List<Structure> structureList = new();
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
    }

    public void AddStrList(Structure str)
    {
        warningListBtn.gameObject.SetActive(true);

        if (!structureList.Contains(str))
            structureList.Add(str);
        warningText.text = structureList.Count.ToString();
    }

    public void RemoveStrList(Structure str)
    {
        if (structureList.Contains(str))
            structureList.Remove(str);
        warningText.text = structureList.Count.ToString();

        if (structureList.Count == 0)
        {
            warningListBtn.gameObject.SetActive(false);
            warningText.text = string.Empty;
        }
    }

    void FocusWarningStr()
    {
        structureList.RemoveAll(x => !x);
        warningText.text = structureList.Count.ToString();

        if (structureList.Count > 0)
        {
            if (!InputManager.instance.isMapOpened)
            {
                count = 0;
                Vector3 pos = structureList[count].transform.position;
                MapCameraController.instance.ToggleMap(pos);
            }
            else
            {
                count = ((count + 1) < structureList.Count) ? count + 1 : 0;
                Vector3 pos = structureList[count].transform.position;
                MapCameraController.instance.SetCamPos(pos, 4);
            }
        }
    }
}
