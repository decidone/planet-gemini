using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> invenUI;
    public GameObject dragSlot;

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of GameManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    public bool OpenedInvenCheck()
    {
        bool isOpened = false;
        foreach (GameObject ui in invenUI)
        {
            if (ui.activeSelf)
                isOpened = true;
        }

        return isOpened;
    }
}
