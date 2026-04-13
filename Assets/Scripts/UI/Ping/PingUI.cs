using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PingUI : MonoBehaviour
{
    public GameObject panel;
    public Transform gridContent;
    public Button backButton;
    public Button closeButton;
    public GameObject iconButtonPrefab;
    public List<PingGroup> pingGroups = new();

    int selectedGroup;
    int selectedSub;
    int currentPage = -1;

    public int SelectedGroup => selectedGroup;
    public int SelectedSub => selectedSub;
    public bool IsOpen => panel.activeSelf;

    #region Singleton
    public static PingUI instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        panel.SetActive(false);
        backButton.onClick.AddListener(Back);
        closeButton.onClick.AddListener(CloseUI);
    }

    public void OpenUI()
    {
        if (IsOpen) return;

        currentPage = -1;
        panel.SetActive(true);
        BuildGroupPage();
        GameManager.instance.onUIChangedCallback?.Invoke(panel);
    }

    public void CloseUI()
    {
        panel.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(panel);
    }

    void Back()
    {
        if (currentPage >= 0)
        {
            currentPage = -1;
            BuildGroupPage();
        }
    }

    void ClearGrid()
    {
        for (int i = gridContent.childCount - 1; i >= 0; i--)
            Destroy(gridContent.GetChild(i).gameObject);
    }

    void BuildGroupPage()
    {
        ClearGrid();
        backButton.gameObject.SetActive(false);

        for (int i = 0; i < pingGroups.Count; i++)
        {
            var group = pingGroups[i];
            Sprite icon = group.categoryIcon;

            int index = i;
            CreateButton(icon, i == selectedGroup, () =>
            {
                currentPage = index;
                BuildIconPage(index);
            });
        }
    }

    void BuildIconPage(int groupIndex)
    {
        ClearGrid();
        backButton.gameObject.SetActive(true);

        var group = pingGroups[groupIndex];
        for (int i = 0; i < group.icons.Count; i++)
        {
            int index = i;
            bool isCurrent = (groupIndex == selectedGroup && i == selectedSub);
            CreateButton(group.icons[i], isCurrent, () =>
            {
                selectedGroup = groupIndex;
                selectedSub = index;
                CloseUI();
            });
        }
    }

    void CreateButton(Sprite icon, bool highlight, Action onClick)
    {
        var go = Instantiate(iconButtonPrefab, gridContent);
        var img = go.GetComponent<Image>();
        var btn = go.GetComponent<Button>();

        if (img != null && icon != null)
            img.sprite = icon;

        if (highlight && img != null)
            img.color = new Color(1f, 1f, 0.7f, 1f);

        if (btn != null)
            btn.onClick.AddListener(() => onClick?.Invoke());
    }
}
