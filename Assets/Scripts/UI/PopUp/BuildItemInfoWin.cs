using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildItemInfoWin : MonoBehaviour
{
    [SerializeField]
    GameObject itemImg;
    [SerializeField]
    RectTransform itemPanelWindow;
    [SerializeField]
    RectTransform energyPanelWindow;
    [SerializeField]
    GameObject panel;
    [SerializeField]
    Image energyBar;
    [SerializeField]
    Text energyText;
    List<GameObject> icon = new List<GameObject>();
    public RectTransform rectTransform;
    public GameObject elsePrefab;
    bool isOverIndex = false;
    int[] widthSize = { 105, 160, 215, 270, 325 };  // 에너지 건물일시 270 이상 세팅해줘야함
    int[] heightSize = { 85, 130 };                 // 에너지 건물일시 130 으로

    public void UiSetting(Dictionary<Item, int> getDic, bool energyUse, bool isEnergyStr, EnergyGroup energyGroup)
    {
        if(getDic != null)
        {
            if(icon.Count != getDic.Count)
            {
                if(icon.Count > getDic.Count)
                {
                    int n = icon.Count - getDic.Count;

                    if (n <= icon.Count)
                    {
                        int startIndex = icon.Count - n;
                        List<GameObject> iconsToRemove = icon.GetRange(startIndex, n);
                        foreach (GameObject obj in iconsToRemove)
                        {
                            Destroy(obj);
                            icon.Remove(obj);
                        }
                    }
                }
                else
                {
                    int n = getDic.Count - icon.Count;

                    for (int i = 0; i < n; i++) 
                    {
                        if (icon.Count < 5)
                        {
                            GameObject itemSlot = Instantiate(itemImg);
                            icon.Add(itemSlot);
                            itemSlot.transform.SetParent(panel.transform, false);
                        }
                        else
                            break;
                    }
                }
            }
            if(getDic.Count > 0 && getDic.Count < 5 && isOverIndex)
            {
                elsePrefab.SetActive(false);
                isOverIndex = false;
            }
        }
        else
        {
            ResetUi();
        }

        UIItemSet(getDic, energyUse, isEnergyStr, energyGroup);
    }

    public void UiSetting(Item item)
    {
        if (icon.Count != 1)
        {
            if (icon.Count > 1)
            {
                int n = icon.Count - 1;

                if (n <= icon.Count)
                {
                    int startIndex = icon.Count - n;
                    List<GameObject> iconsToRemove = icon.GetRange(startIndex, n);
                    foreach (GameObject obj in iconsToRemove)
                    {
                        Destroy(obj);
                        icon.Remove(obj);
                    }
                }
            }
            else
            {
                int n = 1 - icon.Count;

                for (int i = 0; i < n; i++)
                {
                    if (icon.Count < 5)
                    {
                        GameObject itemSlot = Instantiate(itemImg);
                        icon.Add(itemSlot);
                        itemSlot.transform.SetParent(panel.transform, false);
                    }
                    else
                        break;
                }
            }
        }

        UIItemSet(item);
    }

    void UIItemSet(Dictionary<Item, int> getDic, bool energyUse, bool isEnergyStr, EnergyGroup energyGroup)
    {
        if (getDic != null)
        {
            if(getDic.Count < 5)
            {
                if (energyUse || isEnergyStr)
                {
                    float newWidth = widthSize[3];
                    rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
                    itemPanelWindow.sizeDelta = new Vector2(newWidth, itemPanelWindow.sizeDelta.y);
                    float newheight = heightSize[1];
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
                    energyPanelWindow.gameObject.SetActive(true);
                    energyBar.fillAmount = energyGroup.efficiency;

                    if(energyUse)
                    {
                        energyText.text = "Energy";
                    }
                    else if (isEnergyStr)
                    {
                        energyText.text = "Energy : " + energyGroup.consumption + " / " + energyGroup.energy;
                    }
                }
                else
                {
                    float newWidth = widthSize[getDic.Count - 1];
                    rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
                    itemPanelWindow.sizeDelta = new Vector2(newWidth, itemPanelWindow.sizeDelta.y);
                    float newheight = heightSize[0];
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
                    energyPanelWindow.gameObject.SetActive(false);
                }
            }
            else
            {
                if (energyUse || isEnergyStr)
                {
                    float newheight = heightSize[1];
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
                    energyPanelWindow.gameObject.SetActive(true);
                    energyBar.fillAmount = energyGroup.efficiency;

                    if (energyUse)
                    {
                        energyText.text = "Energy";
                    }
                    else if (isEnergyStr)
                    {
                        energyText.text = "Energy : " + energyGroup.consumption + " / " + energyGroup.energy;
                    }
                }
                else
                {
                    float newheight = heightSize[0];
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
                    energyPanelWindow.gameObject.SetActive(false);
                }
                float newWidth = widthSize[4];
                rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
                itemPanelWindow.sizeDelta = new Vector2(newWidth, itemPanelWindow.sizeDelta.y);
            }

            int index = 0;
            foreach (var data in getDic)
            {
                if(index >= 0 && index < 4)
                {
                    Item item = data.Key;
                    int amount = data.Value;
                    icon[index].GetComponent<BuildingImgCtrl>().AddItem(item, amount, true);
                    icon[index].SetActive(true);
                } 
                else if(index >= 4)
                {
                    isOverIndex = true;
                    elsePrefab.transform.SetAsLastSibling();
                    elsePrefab.SetActive(true);
                    break;
                }                    
                index++;
            }
        }
        else
        {
            float newWidth = widthSize[3];
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
            itemPanelWindow.sizeDelta = new Vector2(newWidth, itemPanelWindow.sizeDelta.y);
            float newheight = heightSize[0];
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
            energyPanelWindow.gameObject.SetActive(true);
            energyBar.fillAmount = energyGroup.efficiency;
            energyText.text = "Energy : " + energyGroup.consumption + " / " + energyGroup.energy;
        }        
    }

    void UIItemSet(Item item)
    {
        float newWidth = widthSize[0];
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
        itemPanelWindow.sizeDelta = new Vector2(newWidth, itemPanelWindow.sizeDelta.y);
        float newheight = heightSize[0];
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newheight);
        energyPanelWindow.gameObject.SetActive(false);

        icon[0].GetComponent<BuildingImgCtrl>().AddItem(item);
        icon[0].SetActive(true);
    }

    public void ResetUi()
    {
        if (icon.Count > 0)
        {
            foreach (GameObject UI in icon)
            {
                Destroy(UI);
            }
            icon.Clear();
        }
    }
}
