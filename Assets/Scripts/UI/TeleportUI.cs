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
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        rightBtn.GetComponentInChildren<Text>().text = displayMarketName;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

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
        if (GameManager.instance.isPlayerInHostMap)
        {
            leftBtn.GetComponentInChildren<Text>().text = displayClientWorldName;
        }
        else
        {
            leftBtn.GetComponentInChildren<Text>().text = displayHostWorldName;
        }
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
        }
        else if (GameManager.instance.isPlayerInHostMap)
        {
            displayText.text = displayHostWorldName;
        }
        else
        {
            displayText.text = displayClientWorldName;
        }

    }
}
