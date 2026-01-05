using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoNeedItemUi : MonoBehaviour
{
    public Image icon;
    public Text amount;
    string itemName;
    ItemInfoWindow itemInfoWindow;

    private void Start()
    {
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
    }

    public void DataSet(Sprite sprite, string item)
    {
        icon.sprite = sprite;
        itemName = item;

        EventTrigger trigger = icon.gameObject.GetComponent<EventTrigger>();
        trigger.triggers.RemoveRange(0, trigger.triggers.Count);
        AddEvent(EventTriggerType.PointerEnter, delegate { OnEnter(); });
        AddEvent(EventTriggerType.PointerExit, delegate { OnExit(); });
    }

    public void AmountSet(int saveAmount, int fullAmount)
    {
        amount.text = saveAmount + " / " + fullAmount;
    }

    void AddEvent(EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);

        EventTrigger trigger = icon.gameObject.GetComponent<EventTrigger>();
        trigger.triggers.Add(eventTrigger);
    }

    void OnEnter()
    {
        itemInfoWindow.OpenWindow(itemName);
    }

    void OnExit()
    {
        itemInfoWindow.CloseWindow();
    }
}
