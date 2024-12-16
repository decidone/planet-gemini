using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPortalListItem : MonoBehaviour
{
    [SerializeField] Text portalName;
    [SerializeField] Button focusBtn;
    [SerializeField] Button editBtn;
    [SerializeField] Button teleportBtn;
    float posX;
    float posY;
    float dataPosY;
    GameObject obj;

    public void ItemInit(float _posX, float _posY, bool isHostMap, GameObject _obj)
    {
        obj = _obj;
        posX = _posX;
        posY = _posY;
        dataPosY = posY;

        if (!isHostMap)
            dataPosY -= (MapGenerator.instance.height + MapGenerator.instance.clientMapOffsetY);

        SetListItemName();

        focusBtn.onClick.AddListener(() => MoveCam());
        editBtn.onClick.AddListener(() => OpenEditUI());
        teleportBtn.onClick.AddListener(() => LocalTP());
    }

    public void SetListItemName()
    {
        if (obj.TryGetComponent(out Structure str))
        {
            if (str.portalName != "")
            {
                portalName.text = str.portalName;
            }
            else
            {
                portalName.text = posX + ", " + dataPosY;
            }
        }
    }

    void MoveCam()
    {
        MapCameraController.instance.Move(posX, posY);
    }

    void OpenEditUI()
    {
        LocalPortalListManager portalListManager = LocalPortalListManager.instance;
        portalListManager.OpenEditUI(obj);
    }

    void LocalTP()
    {
        MapCameraController mapCamera = MapCameraController.instance;
        Vector3 pos = new Vector3(posX, posY, 0);
        mapCamera.PlayerLocalTP(pos);
    }
}
