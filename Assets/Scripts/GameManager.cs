using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> invenUI;
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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (hit.collider != null)
            {
                ClickEvent clickEvent = hit.collider.GetComponent<ClickEvent>();
                if (clickEvent != null)
                {
                    clickEvent.OpenUI();
                }
            }
        }
    }
}
