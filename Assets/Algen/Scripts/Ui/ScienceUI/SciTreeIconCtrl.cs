using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SciTreeIconCtrl : MonoBehaviour
{
    public Image icon;
    public Button iconBtn;
    ScienceBtn scienceBtn;
    // Start is called before the first frame update
    void Awake()
    {
        scienceBtn = iconBtn.GetComponent<ScienceBtn>();
    }

    public void SetIcon(string sciName, int level)
    {
        scienceBtn.SetInfo(sciName, level, false);
    }
}
