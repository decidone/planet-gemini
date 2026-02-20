using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BasicUIBtns : MonoBehaviour
{
    [SerializeField]
    GameObject playerBtns;
    [SerializeField]
    GameObject unitBtns;
    [SerializeField]
    Button swapBtn;
    bool isPlayerBtnOn;
    [HideInInspector]
    public bool mouseOnBtn;
    public bool isSwapBtn;
    ItemInfoWindow itemInfoWindow;
    InputManager inputManager;
    [SerializeField]
    BUIBtn[] btns;

    // 시간 UI 관련
    public RectTransform uiPanel;
    [SerializeField]
    GameObject timeGroup;
    Vector3 timeGroupDefaultPos;
    [SerializeField]
    GameObject dDayGroup;
    Vector3 dDayGroupDefaultPos;
    [SerializeField]
    Button[] timeBtns;  // 0 : Up, 1 : Down
    private int currentStage = 0; // 현재 단계 (0~2)
    private float[] positions; // Y 좌표 목록
    private bool isSliding = false; // 슬라이드 중인지 확인
    SoundManager soundManager;

    // 유닛 UI 관련
    public Text unitAmountText;

    // 회전 가능한 건물 UI 관련
    [SerializeField]
    GameObject rotatePanel;
    string rotateKey;
    [SerializeField]
    Text rotateText;
    float timePosY;

    public bool testOff;
    public bool testOn;

    #region Singleton
    public static BasicUIBtns instance;

    private void Awake()
    {
        timeGroupDefaultPos = timeGroup.GetComponent<RectTransform>().localPosition;
        dDayGroupDefaultPos = dDayGroup.GetComponent<RectTransform>().localPosition;

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        soundManager = SoundManager.instance;
        itemInfoWindow = GameManager.instance.inventoryUiCanvas.GetComponent<ItemInfoWindow>();
        SwapFunc(true);
        KeyValueSet();
        swapBtn.onClick.AddListener(() => SwapBtn());
        timePosY = uiPanel.anchoredPosition.y;
        if (!MainGameSetting.instance.isBloodMoon)
        {
            positions = new float[] { timePosY, timePosY - 100f };
            timeGroup.GetComponent<RectTransform>().localPosition = dDayGroupDefaultPos;
            dDayGroup.SetActive(false);
        }
        else
        {
            positions = new float[] { timePosY, timePosY - 100f, timePosY - 200f };
        }
        UpdateButtonState();

        timeBtns[0].onClick.AddListener(() => ChangeStage(-1));
        timeBtns[1].onClick.AddListener(() => ChangeStage(1));
    }

    private void Update()
    {
        if (testOff)
        {
            testOff = false;
            BloodMoonUIOff();
        }

        if (testOn)
        {
            testOn = false;
            BloodMoonUIOn();
        }
    }

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.HotKey.BasicUIBtnsSwap.performed += SwapBtn;
        inputManager.controls.HotKey.TimeUIUp.performed += TimeUIUp;
        inputManager.controls.HotKey.TimeUIDown.performed += TimeUIDown;
    }

    void OnDisable()
    {
        inputManager.controls.HotKey.BasicUIBtnsSwap.performed -= SwapBtn;
        inputManager.controls.HotKey.TimeUIUp.performed -= TimeUIUp;
        inputManager.controls.HotKey.TimeUIDown.performed -= TimeUIDown;
    }

    public void KeyValueSet()
    {
        Dictionary<string, InputAction> actions = SettingsMenu.instance.inputActions;
        foreach (BUIBtn btn in btns)
        {
            if (!btn.isStickyKey)
            {
                foreach (var data in actions)
                {
                    if (data.Key == btn.OptionName)
                    {
                        var playerInvenAction = data.Value.bindings[0].effectivePath;
                        string key = InputControlPath.ToHumanReadableString(playerInvenAction, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        btn.KeyValueSet(key);
                        break;
                    }
                }
            }
            else
                continue;
        }

        foreach (var data in actions)
        {
            if (data.Key == "Rotate")
            {
                rotateKey = InputControlPath.ToHumanReadableString(data.Value.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                if (rotatePanel.activeSelf)
                    SetRotateUI(true);
                break;
            }
        }
    }

    void SwapBtnFunc()
    {
        if (playerBtns.activeSelf)
        {
            SwapFunc(false);
        }
        else
        {
            SwapFunc(true);
        }
    }

    void SwapBtn(InputAction.CallbackContext ctx)
    {
        SwapBtn();
    }

    void SwapBtn()
    {
        SwapBtnFunc();
        if (mouseOnBtn && isSwapBtn)
        {
            itemInfoWindow.CloseWindow();
        }
        soundManager.PlayUISFX("SidebarClick");
    }

    public void UnitCtrlSwapBtn()
    {
        if (isPlayerBtnOn)
        {
            SwapFunc(false);
        }
    }

    public void SwapFunc(bool playerBtnOn)
    {
        isPlayerBtnOn = playerBtnOn;

        playerBtns.SetActive(isPlayerBtnOn);
        unitBtns.SetActive(!isPlayerBtnOn);
    }

    public void OnExit()
    {
        mouseOnBtn = false;
        isSwapBtn = false;
        itemInfoWindow.CloseWindow();
    }

    void TimeUIUp(InputAction.CallbackContext ctx)
    {
        ChangeStage(-1);
    }

    void TimeUIDown(InputAction.CallbackContext ctx)
    {
        ChangeStage(1);
    }

    private void ChangeStage(int direction)
    {
        if (isSliding) return; // 슬라이드 중이면 클릭 방지
        soundManager.PlayUISFX("SidebarClick");

        int newStage = Mathf.Clamp(currentStage + direction, 0, positions.Length - 1);
        if (newStage != currentStage)
        {
            StartCoroutine(SlideUI(positions[newStage]));
            currentStage = newStage;
            UpdateButtonState();
        }
    }

    private IEnumerator SlideUI(float targetY)
    {
        isSliding = true; // 슬라이드 중 버튼 비활성화

        float duration = 0.3f; // 슬라이드 시간
        float elapsed = 0f;
        Vector2 startPos = uiPanel.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            uiPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        uiPanel.anchoredPosition = targetPos; // 최종 위치 고정
        isSliding = false; // 슬라이드 종료 후 버튼 활성화
    }

    private void UpdateButtonState()
    {
        timeBtns[0].interactable = currentStage > 0;
        timeBtns[1].interactable = currentStage < positions.Length - 1;
    }

    public void SetRotateUI(bool isActive)
    {
        rotatePanel.SetActive(isActive);
        if (isActive)
            rotateText.text = "Press ('" + rotateKey + "') to rotate.";
    }

    public void BloodMoonUIOn()
    {
        positions = new float[] { timePosY, timePosY - 100f, timePosY - 200f };
        timeGroup.GetComponent<RectTransform>().localPosition = timeGroupDefaultPos;
        dDayGroup.SetActive(true);
        ChangeStage(1);
    }

    public void BloodMoonUIOff()
    {
        positions = new float[] { timePosY, timePosY - 100f };
        timeGroup.GetComponent<RectTransform>().localPosition = dDayGroupDefaultPos;
        dDayGroup.SetActive(false);
        ChangeStage(-1);
    }
}
