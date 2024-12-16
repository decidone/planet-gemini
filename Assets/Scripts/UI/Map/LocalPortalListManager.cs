using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPortalListManager : MonoBehaviour
{
    [SerializeField] GameObject displayObj;
    [SerializeField] GameObject localPortalListItemsObj;
    [SerializeField] GameObject localPortalListItemPref;
    [Space]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] Button okBtn;
    [SerializeField] Button cancelBtn;
    [SerializeField] InputField inputField;
    List<GameObject> localPortalListItems = new List<GameObject>();

    public bool isEditOpened;

    #region Singleton
    public static LocalPortalListManager instance;

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
        cancelBtn.onClick.AddListener(() => CloseEditUI());
    }

    public void OpenUI()
    {
        displayObj.SetActive(true);

        foreach (Portal portal in NetworkObjManager.instance.netPortals)
        {
            if (portal.isInHostMap == GameManager.instance.isPlayerInHostMap)
            {
                GameObject portalListItemObj = Instantiate(localPortalListItemPref, localPortalListItemsObj.transform);
                LocalPortalListItem portalListItem = portalListItemObj.GetComponent<LocalPortalListItem>();
                float posX = portal.transform.position.x;
                float posY = portal.transform.position.y;
                portalListItem.ItemInit(posX, posY, portal.isInHostMap, portal.gameObject);

                localPortalListItems.Add(portalListItemObj);
            }
        }

        foreach (Structure structure in NetworkObjManager.instance.netStructures)
        {
            if (structure.TryGetComponent(out LocalPortal localPortal))
            {
                if (localPortal.isInHostMap == GameManager.instance.isPlayerInHostMap)
                {
                    GameObject localPortalListItemObj = Instantiate(localPortalListItemPref, localPortalListItemsObj.transform);
                    LocalPortalListItem portalListItem = localPortalListItemObj.GetComponent<LocalPortalListItem>();
                    float posX = localPortal.transform.position.x;
                    float posY = localPortal.transform.position.y;
                    portalListItem.ItemInit(posX, posY, localPortal.isInHostMap, localPortal.gameObject);

                    localPortalListItems.Add(localPortalListItemObj);
                }
            }
        }
    }

    public void CloseUI()
    {
        displayObj.SetActive(false);
        CloseEditUI();
        DestroyListItems();
    }

    public void OpenEditUI(GameObject obj)
    {
        confirmPanel.SetActive(true);
        isEditOpened = true;
        okBtn.onClick.AddListener(() => SetPortalName(obj));
    }

    void SetPortalName(GameObject obj)
    {
        if (obj.TryGetComponent(out Structure str))
        {
            str.SetPortalName(inputField.text);
            RefreshListItemName();
        }

        CloseEditUI();
    }

    void RefreshListItemName()
    {
        foreach (var obj in localPortalListItems)
        {
            if (obj.TryGetComponent(out LocalPortalListItem item))
            {
                item.SetListItemName();
            }
        }
    }

    public void CloseEditUI()
    {
        confirmPanel.SetActive(false);
        isEditOpened = false;
        inputField.text = "";
        okBtn.onClick.RemoveAllListeners();
    }

    public void DestroyListItems()
    {
        if (localPortalListItems.Count == 0) return;

        foreach (GameObject portalListItem in localPortalListItems)
        {
            Destroy(portalListItem);
        }
        localPortalListItems.Clear();
    }
}
