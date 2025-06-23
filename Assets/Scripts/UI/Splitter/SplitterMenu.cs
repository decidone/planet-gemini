using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplitterMenu : MonoBehaviour
{
    [SerializeField]
    Sprite[] splitterSprites;
    [SerializeField]
    Image Image;
    [SerializeField]
    GameObject[] tags;
    public Slot[] slots;
    public Button[] fillterMenuBtns;
    public ToggleButton[] fillterOnOffBtns;
    public ToggleButton[] reverseToggle;

    public (Slot[], Button[], ToggleButton[], ToggleButton[]) SetMenu(int dirIndex)
    {
        Image.sprite = splitterSprites[dirIndex];

        int disableUIIndex = dirIndex - 1;
        if (disableUIIndex < 0) disableUIIndex = 3;

        List<Slot> slotArr = new List<Slot>();
        List<Button> fillterMenuBtnArr = new List<Button>();
        List<ToggleButton> fillterOnOffBtnArr = new List<ToggleButton>();
        List<ToggleButton> reverseToggleArr = new List<ToggleButton>();

        for (int i = 1; i <= 3; i++)
        {
            int idx = (disableUIIndex + i) % 4;

            if(slots.Length > 0)
                slotArr.Add(slots[idx]);
            if (fillterMenuBtns.Length > 0)
                fillterMenuBtnArr.Add(fillterMenuBtns[idx]);
            if (fillterOnOffBtns.Length > 0)
                fillterOnOffBtnArr.Add(fillterOnOffBtns[idx]);
            if (reverseToggle.Length > 0)
                reverseToggleArr.Add(reverseToggle[idx]);

            tags[idx].gameObject.SetActive(true);
        }

        // 비활성화 대상만 끄기
        tags[disableUIIndex].gameObject.SetActive(false);

        return (
            slotArr.ToArray(),
            fillterMenuBtnArr.ToArray(),
            fillterOnOffBtnArr.ToArray(),
            reverseToggleArr.ToArray()
        );
    }
}
