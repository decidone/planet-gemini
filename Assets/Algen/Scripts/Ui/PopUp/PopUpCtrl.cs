using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpCtrl : MonoBehaviour
{
    public GameManager gameManager;
    [SerializeField]
    protected Button okBtn;
    [SerializeField]
    protected Button cancelBtn;
    [SerializeField]
    protected Text pupUpText;
    protected string pupUpContent;

    protected virtual void Awake()
    {
        gameManager = GameManager.instance;
    }

    void Start()
    {
        if(okBtn != null)
        {
            okBtn.onClick.AddListener(OkBtnFunc);
        }
        if (cancelBtn != null)
        {
            cancelBtn.onClick.AddListener(CancelBtnFunc);
        }
    }

    protected virtual void OkBtnFunc() { }
    protected virtual void CancelBtnFunc() { }

    public virtual void OpenUI() 
    { 
        gameObject.SetActive(true);
    }

    public virtual void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
