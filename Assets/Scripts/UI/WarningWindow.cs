using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningWindow : MonoBehaviour
{
    [SerializeField]
    GameObject imgPanel;
    [SerializeField]
    Text warningText;
    bool warningStart;
    float setTimer;
    float maxTimer;

    #region Singleton
    public static WarningWindow instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of ItemDragManager found!");
            return;
        }
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        maxTimer = 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (warningStart)
        {
            setTimer += Time.deltaTime;

            if (setTimer > maxTimer)
            {
                WarningState(false);
            }
        }
    }

    public void WarningTextSet(string text)
    {
        warningText.text = text;
        WarningState(true);
    }

    public void WarningTextSet(string text, bool isHostMap)
    {
        string mapselect;
        if (isHostMap)
            mapselect = "Map 1.";
        else
            mapselect = "Map 2.";

        warningText.text = text + " " + mapselect;
        WarningState(true);
    }

    void WarningState(bool start)
    {
        imgPanel.SetActive(start);
        warningStart = start;
        setTimer = 0;
    }
}
