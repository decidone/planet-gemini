using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bounty : MonoBehaviour
{
    Overall overall;
    [SerializeField] Finance finance;
    [SerializeField] Button btn;
    [SerializeField] Text spawnerText;
    [SerializeField] Text monsterText;

    public int bounty;
    int newSpawnerCount;
    int newMonsterCount;

    private void Awake()
    {
        btn.onClick.AddListener(ReceiveBounty);
    }

    public void OpenUI()
    {
        this.gameObject.SetActive(true);
        GameManager.instance.onUIChangedCallback?.Invoke(this.gameObject);
        overall = Overall.instance;
        newSpawnerCount = overall.spawnerDestroyCount - overall.spawnerBountyReceived;
        newMonsterCount = overall.monsterKillCount - overall.monsterBountyReceived;
        spawnerText.text = overall.spawnerBountyReceived + "";
        if (newSpawnerCount != 0)
            spawnerText.text += " +" + newSpawnerCount;
        monsterText.text = overall.monsterBountyReceived + "";
        if (newMonsterCount != 0)
            monsterText.text += " +" + newMonsterCount;

        bounty = (overall.spawnerDestroyCount - overall.spawnerBountyReceived) * 100;
        bounty += (overall.monsterKillCount - overall.monsterBountyReceived) * 5;
        finance.SetFinance(bounty);
        btn.interactable = bounty > 0;
    }

    public void CloseUI()
    {
        this.gameObject.SetActive(false);
        GameManager.instance.onUIChangedCallback?.Invoke(this.gameObject);
    }

    public void ReceiveBounty()
    {
        GameManager.instance.AddFinanceServerRpc(bounty);
        overall.ReceivedCount(0, newSpawnerCount);
        overall.ReceivedCount(1, newMonsterCount);
        finance.SetFinance(0);
        bounty = 0;
        newSpawnerCount = 0;
        newMonsterCount = 0;
        spawnerText.text = overall.spawnerBountyReceived.ToString();
        monsterText.text = overall.monsterBountyReceived.ToString();
        btn.interactable = false;
        SoundManager.instance.PlayUISFX("ButtonClick");
    }
}
