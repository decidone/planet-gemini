using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] Text stateText;
    [SerializeField] Button btn;
    [SerializeField] Text btnText;

    #region Singleton
    public static ShopPopup instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void OpenUI()
    {
        obj.SetActive(true);
        GameManager.instance.onUIChangedCallback?.Invoke(obj);
        btn.onClick.RemoveAllListeners();
    }

    public void CloseUI()
    {
        obj.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(obj);
    }

    public void NotEnoughSpacePopup()
    {
        OpenUI();
        stateText.text = "Not enough space";
        btn.onClick.AddListener(CloseUI);
        btnText.text = "Close";
    }
}
