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
        hotspot = new Vector2(16, 16);
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
            if (texture == cursorSkin.buildingCursor)
            {
                Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
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

    public void BuildingCursorSet()
    {
        CursorSetting(cursorSkin.buildingCursor);
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
        if (UpgradeRemoveBtn.instance.clickBtn)
        {
            if (UpgradeRemoveBtn.instance.btnSwitch)
            {
                DragCursorSet(false);
            }
            else
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
