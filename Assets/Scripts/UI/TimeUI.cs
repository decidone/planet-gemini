using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField]
    Sprite[] timeImgSet;
    [SerializeField]
    Image timeImg;
    [SerializeField]
    Text timeText;
    [SerializeField]
    Text dayText;

    #region Singleton
    public static TimeUI instance;

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

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DayText(int day)
    {
        dayText.text = "Day : " + day;
    }
}
