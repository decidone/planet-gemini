using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapFilter : MonoBehaviour
{
    [SerializeField] Button button;
    public bool isFilterOn;

    #region Singleton
    public static MapFilter instance;

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
        button.onClick.AddListener(FilterToggle);
    }

    void FilterToggle()
    {
        if (isFilterOn)
        {
            MapCameraController.instance.cam.cullingMask |= 1 << LayerMask.NameToLayer("MapUI");
            isFilterOn = false;
        }
        else
        {
            MapCameraController.instance.cam.cullingMask = MapCameraController.instance.cam.cullingMask & ~(1 << LayerMask.NameToLayer("MapUI"));
            isFilterOn = true;
        }
    }

    public void OpenUI()
    {
        button.gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        button.gameObject.SetActive(false);
        if (isFilterOn)
        {
            MapCameraController.instance.cam.cullingMask |= 1 << LayerMask.NameToLayer("MapUI");
            isFilterOn = false;
        }
    }
}
