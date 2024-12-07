using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// UTF-8 설정
public class SciTreeIconCtrl : MonoBehaviour
{
    public Image icon;
    public Button iconBtn;
    ScienceBtn scienceBtn;

    void Awake()
    {
        scienceBtn = iconBtn.GetComponent<ScienceBtn>();
    }

    public void SetIcon(string sciName, int level, int coreLv, float time, string gameName, bool basicScience)
    {
        scienceBtn.SetInfo(sciName, level, coreLv, time, false, gameName, basicScience);
    }
}
