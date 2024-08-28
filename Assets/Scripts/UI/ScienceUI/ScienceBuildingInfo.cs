using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScienceBuildingInfo : MonoBehaviour
{
    [SerializeField]
    Button okBtn;
    [SerializeField]
    GameObject panel;
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.instance;
        okBtn.onClick.AddListener(() => CloseUI());
    }

    public void OpenUI()
    {
        panel.SetActive(true);
        gameManager.onUIChangedCallback?.Invoke(panel);
    }

    public void CloseUI()
    {
        panel.SetActive(false);
        gameManager.onUIChangedCallback?.Invoke(panel);
    }
}
