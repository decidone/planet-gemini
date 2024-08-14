using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    [SerializeField] QuestListSO questListSO;
    List<Quest> quests;

    [SerializeField] Text titleText;
    [SerializeField] Text descriptionText;

    int currentQuest;
    Overall overall;
    NetworkObjManager networkObjManager;
    ScienceManager scienceManager;

    #region Singleton
    public static QuestManager instance;

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
        quests = questListSO.QuestSOList;
        overall = Overall.instance;
        networkObjManager = NetworkObjManager.instance;
        scienceManager = ScienceManager.instance;
        currentQuest = 0;

        //SetQuest(currentQuest);
    }

    void ResetUI()
    {
        titleText.text = "";
        descriptionText.text = "";
    }

    public void SetQuest(int order)
    {
        if (quests.Count <= order)
            return;

        PlayerController player = GameManager.instance.player.GetComponent<PlayerController>();

        titleText.text = quests[order].title;
        descriptionText.text = quests[order].description;

        switch (quests[order].type)
        {
            case 0:
                player.onTeleportedCallback += TeleportCheck;
                break;
            case 1:
                if (overall.OverallSentCheck() || overall.OverallReceivedCheck())
                    QuestClear();
                else
                    overall.onOverallChangedCallback += QuestCompCheck;
                break;
            case 10:
                if (quests[order].item != null)
                {
                    int itemAmount = overall.OverallPurchasedItemCheck(quests[order].item);
                    descriptionText.text = quests[order].description + "\n" + itemAmount + " / " + quests[order].amount;
                    if (quests[order].amount <= itemAmount)
                        QuestClear();
                    else
                        overall.onOverallChangedCallback += QuestCompCheck;
                }
                break;
            case 11:
                if (quests[order].item != null)
                {
                    int itemAmount = overall.OverallSoldItemCheck(quests[order].item);
                    descriptionText.text = quests[order].description + "\n" + itemAmount + " / " + quests[order].amount;
                    if (quests[order].amount <= itemAmount)
                        QuestClear();
                    else
                        overall.onOverallChangedCallback += QuestCompCheck;
                }
                break;
            case 20:
                if (networkObjManager.StructureCheck(quests[order].strData))
                    QuestClear();
                else
                    networkObjManager.onStructureChangedCallback += QuestCompCheck;
                break;
            case 21:
                if (quests[order].item != null)
                {
                    int itemAmount = overall.OverallProdItemCheck(quests[order].item);
                    descriptionText.text = quests[order].description + "\n" + itemAmount + " / " + quests[order].amount;
                    if (quests[order].amount <= itemAmount)
                        QuestClear();
                    else
                        overall.onOverallChangedCallback += QuestCompCheck;
                }
                break;
            case 22:
                if (networkObjManager.UnitCheck(quests[order].unitData))
                    QuestClear();
                else
                    networkObjManager.onUnitChangedCallback += QuestCompCheck;
                break;
            case 30:
                descriptionText.text = quests[order].description + "\n" + overall.spawnerDestroyCount + " / " + quests[order].amount;
                if (quests[order].amount <= overall.spawnerDestroyCount)
                    QuestClear();
                else
                    overall.onOverallChangedCallback += QuestCompCheck;
                break;
            case 31:
                descriptionText.text = quests[order].description + "\n" + overall.monsterKillCount + " / " + quests[order].amount;
                if (quests[order].amount <= overall.monsterKillCount)
                    QuestClear();
                else
                    overall.onOverallChangedCallback += QuestCompCheck;
                break;
            case 40:
                if (scienceManager.isAnyUpgradeCompleted)
                    QuestClear();
                else
                    scienceManager.onUpgradeCompletedCallback += QuestCompCheck;
                break;
            default:
                break;
        }
    }

    public void TeleportCheck(int type)
    {
        if (type == 0 && quests[currentQuest].destination == 0)
        {
            PlayerController player = GameManager.instance.player.GetComponent<PlayerController>();
            player.onTeleportedCallback -= TeleportCheck;
            QuestClear();
        }
        else if (type == 1 && quests[currentQuest].destination == 1)
        {
            PlayerController player = GameManager.instance.player.GetComponent<PlayerController>();
            player.onTeleportedCallback -= TeleportCheck;
            QuestClear();
        }
    }

    public void QuestCompCheck(int overallType)
    {
        if (overallType != quests[currentQuest].type)
            return;

        switch (quests[currentQuest].type)
        {
            case 1:
                if (overall.OverallSentCheck() || overall.OverallReceivedCheck())
                {
                    overall.onOverallChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 10:
                int purchasedAmount = overall.OverallPurchasedItemCheck(quests[currentQuest].item);
                descriptionText.text = quests[currentQuest].description + "\n" + purchasedAmount + " / " + quests[currentQuest].amount;
                if (quests[currentQuest].amount <= purchasedAmount)
                {
                    overall.onOverallChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 11:
                int soldAmount = overall.OverallSoldItemCheck(quests[currentQuest].item);
                descriptionText.text = quests[currentQuest].description + "\n" + soldAmount + " / " + quests[currentQuest].amount;
                if (quests[currentQuest].amount <= soldAmount)
                {
                    overall.onOverallChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 20:
                if (networkObjManager.StructureCheck(quests[currentQuest].strData))
                {
                    networkObjManager.onStructureChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 21:
                int prodAmount = overall.OverallProdItemCheck(quests[currentQuest].item);
                descriptionText.text = quests[currentQuest].description + "\n" + prodAmount + " / " + quests[currentQuest].amount;
                if (quests[currentQuest].amount <= prodAmount)
                {
                    overall.onOverallChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 22:
                if (networkObjManager.UnitCheck(quests[currentQuest].unitData))
                {
                    networkObjManager.onUnitChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 30:
                descriptionText.text = quests[currentQuest].description + "\n" + overall.spawnerDestroyCount + " / " + quests[currentQuest].amount;
                if (quests[currentQuest].amount <= overall.spawnerDestroyCount)
                {
                    networkObjManager.onUnitChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 31:
                descriptionText.text = quests[currentQuest].description + "\n" + overall.monsterKillCount + " / " + quests[currentQuest].amount;
                if (quests[currentQuest].amount <= overall.monsterKillCount)
                {
                    networkObjManager.onUnitChangedCallback -= QuestCompCheck;
                    QuestClear();
                }
                break;
            case 40:
                scienceManager.onUpgradeCompletedCallback -= QuestCompCheck;
                QuestClear();
                break;
            default:
                break;
        }
    }

    public void QuestClear()
    {
        ResetUI();

        quests[currentQuest].isCompleted = true;
        currentQuest++;
        SetQuest(currentQuest);
    }
}
