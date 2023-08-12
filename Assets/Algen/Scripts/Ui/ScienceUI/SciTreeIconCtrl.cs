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
    void Start()
    {
        scienceBtn = iconBtn.GetComponent<ScienceBtn>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIcon(string sciName, int level)
    {
        scienceBtn.sciName = sciName;
        scienceBtn.level = level;
    }
}
