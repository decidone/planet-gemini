using UnityEngine;

public class MouseSkin : MonoBehaviour
{
    public CursorSkinSO cursorSkin;
    public Texture2D setTexture;
    public Texture2D tempTexture;
    Vector2 hotspot;
    bool isOnUI;
    #region Singleton
    public static MouseSkin instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        hotspot = new Vector2(16, 12);
        CursorSetting(cursorSkin.baseCursor);
    }

    void Update()
    {
        if (RaycastUtility.IsPointerOverUI(Input.mousePosition) && !isOnUI)
        {
            isOnUI = true;
            CursorSetting(cursorSkin.baseCursor);
        }
        else if (!RaycastUtility.IsPointerOverUI(Input.mousePosition) && isOnUI)
        {
            isOnUI = false;
            CursorSetting(tempTexture);
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if (!focus && UpgradeRemoveBtn.instance != null)
        {
            ResetCursor();
        }
    }

    void CursorSetting(Texture2D texture)
    {
        setTexture = texture;

        if (cursorSkin.baseCursor != texture && tempTexture != texture)
        {
            tempTexture = setTexture;
        }

        if (!RaycastUtility.IsPointerOverUI(Input.mousePosition))
        {
            if (texture == cursorSkin.buildingCursor[0] || texture == cursorSkin.buildingCursor[1])
            {
                Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            }
            else if (texture == cursorSkin.dragCursor[0])
            {
                Cursor.SetCursor(texture, new Vector2(2, 12), CursorMode.Auto);
            }
            else if (texture == cursorSkin.dragCursor[1])
            {
                Cursor.SetCursor(texture, new Vector2(4, 12), CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
            }
        }
        else
        {
            Cursor.SetCursor(cursorSkin.baseCursor, hotspot, CursorMode.Auto);
        }
    }

    public void DragCursorSet(bool state)
    {
        if (state)
            CursorSetting(cursorSkin.dragCursor[0]);
        else
            CursorSetting(cursorSkin.dragCursor[1]);
    }

    public void BuildingCursorSet(int index)
    {
        CursorSetting(cursorSkin.buildingCursor[index]);
    }

    public void UnitCursorCursorSet(bool state)
    {
        if (state)
            CursorSetting(cursorSkin.unitCursor[0]);
        else
            CursorSetting(cursorSkin.unitCursor[1]);
    }

    public void ResetCursor()
    {
        if (UpgradeRemoveBtn.instance.currentBtn != UpgradeRemoveBtn.SelectedButton.None)
        {
            if (UpgradeRemoveBtn.instance.currentBtn == UpgradeRemoveBtn.SelectedButton.BuildingUpgrade)
            {
                DragCursorSet(false);
            }
            if (UpgradeRemoveBtn.instance.currentBtn == UpgradeRemoveBtn.SelectedButton.BuildingRemove ||
                UpgradeRemoveBtn.instance.currentBtn == UpgradeRemoveBtn.SelectedButton.UnitRemove)
            {
                DragCursorSet(true);
            }
        }
        else
        {
            CursorSetting(cursorSkin.baseCursor);
            tempTexture = setTexture;
        }
    }
}
