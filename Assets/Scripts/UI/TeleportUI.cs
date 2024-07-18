using UnityEngine;
using UnityEngine.UI;

public class TeleportUI : MonoBehaviour
{
    [SerializeField] GameObject content;
    public Button leftBtn;
    public Button rightBtn;
    public Text displayText;
    [SerializeField] float displayTime;
    [SerializeField] string displayMarketName;
    [SerializeField] string displayHostWorldName;
    [SerializeField] string displayClientWorldName;
    float timer;
    bool timerRun;

    #region Singleton
    public static TeleportUI instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of TeleportUI found!");
            return;
        }

        instance = this;
    }
    #endregion

    private void Update()
    {
        if (timerRun)
        {
            timer += Time.deltaTime;
            if (timer > displayTime)
            {
                timerRun = false;
                displayText.text = "";
            }
        }
    }

    public void OpenUI()
    {
        content.SetActive(true);
    }

    public void CloseUI()
    {
        content.SetActive(false);
    }

    public void SetBtnDefault()
    {
        leftBtn.onClick.RemoveAllListeners();
        rightBtn.onClick.RemoveAllListeners();
    }

    public void DisplayWorldName()
    {
        timerRun = true;
        timer = 0;

        if (GameManager.instance.isPlayerInMarket)
        {
            displayText.text = displayMarketName;
        }else if (GameManager.instance.isPlayerInHostMap)
        {
            displayText.text = displayHostWorldName;
        }
        else
        {
            displayText.text = displayClientWorldName;
        }

    }
}
