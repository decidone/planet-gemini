using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildItemInfoWin : MonoBehaviour
{
    [SerializeField]
    GameObject itemImg;
    [SerializeField]
    GameObject panel;
    List<GameObject> icon = new List<GameObject>();
    public RectTransform rectTransform;
    public GameObject elsePrefab;
    bool isOverIndex = false;
    int[] wideSize = { 105, 160, 215, 270, 325 };

    public void UiSetting(Dictionary<Item, int> getDic)
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

        UIItemSet(getDic);
    }

    void UIItemSet(Dictionary<Item, int> getDic)
    {
        if (getDic.Count > 0)
        {
            if(getDic.Count < 5)
            {
                float newWidth = wideSize[getDic.Count - 1];
                rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
            }
            else
            {
                float newWidth = wideSize[4];
                rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
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
