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
        spawnerText.text = overall.spawnerBountyReceived + " +" + newSpawnerCount;
        monsterText.text = overall.monsterBountyReceived + " +" + newMonsterCount;

        bounty = (overall.spawnerDestroyCount - overall.spawnerBountyReceived) * 100;
        bounty += (overall.monsterKillCount - overall.monsterBountyReceived) * 5;
        finance.SetFinance(bounty);
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
        spawnerText.text = overall.spawnerBountyReceived.ToString();
        monsterText.text = overall.monsterBountyReceived.ToString();
        finance.SetFinance(0);
    }
}
