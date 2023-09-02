using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UTF-8 설정
public class DragSlot : MonoBehaviour
{
    [SerializeField]
    GameObject slotObj;
    public Slot slot;

    #region Singleton
    public static DragSlot instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of drag slot found!");
            return;
        }

        instance = this;
        slotObj = this.transform.Find("Slot").gameObject;
        slot = slotObj.transform.GetComponent<Slot>();

        Image[] images = slot.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.raycastTarget = false;
        }
    }
    #endregion
}
