using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    public GameObject slotObj;
    public InventorySlot slot;

    public static DragSlot instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of drag slot found!");
            return;
        }

        instance = this;

        slotObj = this.transform.Find("Slot").gameObject;
        slot = slotObj.transform.GetComponent<InventorySlot>();

        Image[] images = slot.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.raycastTarget = false;
        }
    }
}
