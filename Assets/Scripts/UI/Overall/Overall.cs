using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Overall : NetworkBehaviour
{
    /*
     *  1. 생산한 아이템 종류별 총 수량 + 구매, 판매한 경우도 따로 수집(분해도 판매로 취급)
        2. 사용한 아이템 - 이건 굳이?
        3. 생산한 유닛 종류별 수량
        4. 파괴한 스포너 티어, 수량
        5. 파괴한 적 유닛 수
        6. 상대 행성에 보낸 아이템 종류, 수량
     * 
     * ui 텍스트에 연동시킬 콜백도 하나 필요 - ui용 오버롤 매니저 하나 만들어서 캔버스에 박아두고 이쪽 콜백에 연결시키면 될 듯
     * so 리스트가 있는 경우 그거 가져와서 정리
     * 
     */

    public delegate void OnOverallChanged(int type);
    public OnOverallChanged onOverallChangedCallback;

    [SerializeField] ItemListSO itemListSO;
    List<Item> itemList;

    // 아래 딕셔너리들 key 값은 매칭되는 listSO의 순번
    Dictionary<int, int> itemsProduction = new Dictionary<int, int>();          // 생산한 아이템 종류별 총합
    Dictionary<int, int> itemsConsumption = new Dictionary<int, int>();         // 소모한 아이템 종류별 총합
    Dictionary<int, int> purchasedItems = new Dictionary<int, int>();           // 구매한 아이템 종류별 총합
    Dictionary<int, int> soldItems = new Dictionary<int, int>();                // 판매/분해한 아이템 종류별 총합
    Dictionary<int, int> itemsFromHostToClient = new Dictionary<int, int>();    // 호스트 행성에서 클라이언트 행성으로 전송한 아이템 종류별 총합
    Dictionary<int, int> itemsFromClientToHost = new Dictionary<int, int>();    // 클라이언트 행성에서 호스트 행성으로 전송한 아이템 종류별 총합
    public int spawnerDestroyCount;   // 스포너 파괴 카운트 (스포너 단계에 따른 분류는 따로 하지 않음)
    public int spawnerBountyReceived; // 스포너 파괴 보상 카운트
    public int monsterKillCount;      // 몬스터 킬 카운트 (마찬가지로 몬스터 종류에 따른 분류는 따로 하지 않음)
    public int monsterBountyReceived; // 몬스터 킬 보상 카운트

    OverallDisplay display;

    #region Singleton
    public static Overall instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        itemList = itemListSO.itemSOList;
        spawnerDestroyCount = 0;
        spawnerBountyReceived = 0;
        monsterKillCount = 0;
        monsterBountyReceived = 0;

        Init();

        instance = this;
    }
    #endregion

    void Start()
    {
        display = OverallDisplay.instance;
    }

    void OnEnable()
    {
        InputManager.instance.controls.HotKey.Overall.performed += DisplayOverall;
    }

    void OnDisable()
    {
        InputManager.instance.controls.HotKey.Overall.performed += DisplayOverall;
    }

    void Init()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemsProduction.Add(i, 0);
            itemsConsumption.Add(i, 0);
            purchasedItems.Add(i, 0);
            soldItems.Add(i, 0);
            itemsFromHostToClient.Add(i, 0);
            itemsFromClientToHost.Add(i, 0);
        }
    }

    public void DisplayOverall(InputAction.CallbackContext ctx)
    {
        DisplayOverall();
    }

    public void DisplayOverall()
    {
        display.ToggleUI();
    }

    public void OverallProd(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallProdServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallProdServerRpc(int itemIndex, int amount)
    {
        OverallProdClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallProdClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (itemsProduction.ContainsKey(i))
                {
                    itemsProduction[i] += amount;
                    display.SetProdAmount(i, itemsProduction[i]);

                    onOverallChangedCallback?.Invoke(21);
                }
                break;
            }
        }
    }

    public void OverallConsumption(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallConsumptionServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallConsumptionServerRpc(int itemIndex, int amount)
    {
        OverallConsumptionClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallConsumptionClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (itemsConsumption.ContainsKey(i))
                {
                    itemsConsumption[i] += amount;
                    display.SetConsumptionAmount(i, itemsConsumption[i]);
                }
                break;
            }
        }
    }

    public void OverallConsumptionCancel(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallConsumptionCancelServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallConsumptionCancelServerRpc(int itemIndex, int amount)
    {
        OverallConsumptionCancelClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallConsumptionCancelClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (itemsConsumption.ContainsKey(i))
                {
                    itemsConsumption[i] -= amount;
                    if (itemsConsumption[i] < 0)
                        itemsConsumption[i] = 0;
                    display.SetConsumptionAmount(i, itemsConsumption[i]);
                }
                break;
            }
        }
    }

    public void OverallPurchased(Item item, int amount)
    {
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallPurchasedServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallPurchasedServerRpc(int itemIndex, int amount)
    {
        OverallPurchasedClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallPurchasedClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (purchasedItems.ContainsKey(i))
                {
                    purchasedItems[i] += amount;
                    display.SetPurchasedAmount(i, purchasedItems[i]);

                    onOverallChangedCallback?.Invoke(10);
                }
                break;
            }
        }
    }

    public void OverallSold(Item item, int amount)
    {
        // 아이템 판매 & 분쇄
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallSoldServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallSoldServerRpc(int itemIndex, int amount)
    {
        OverallSoldClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallSoldClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (soldItems.ContainsKey(i))
                {
                    soldItems[i] += amount;
                    display.SetSoldAmount(i, soldItems[i]);

                    onOverallChangedCallback?.Invoke(11);
                }
                break;
            }
        }
    }

    public void OverallSent(Item item, int amount)
    {
        // 호스트가 클라이언트에게 아이템 전송
        // ui에 표시할 때 호스트인지 클라이언트인지 잘 확인해서 반대로 표시
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallSentServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallSentServerRpc(int itemIndex, int amount)
    {
        OverallSentClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallSentClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (itemsFromHostToClient.ContainsKey(i))
                {
                    itemsFromHostToClient[i] += amount;
                    if (GameManager.instance.isHost)
                        display.SetSentAmount(i, itemsFromHostToClient[i]);
                    else
                        display.SetReceivedAmount(i, itemsFromHostToClient[i]);

                    onOverallChangedCallback?.Invoke(1);
                }
                break;
            }
        }
    }

    public void OverallReceived(Item item, int amount)
    {
        // 클라이언트가 호스트에게 아이템 전송
        int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(item);
        OverallReceivedServerRpc(itemIndex, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallReceivedServerRpc(int itemIndex, int amount)
    {
        OverallReceivedClientRpc(itemIndex, amount);
    }

    [ClientRpc]
    public void OverallReceivedClientRpc(int itemIndex, int amount)
    {
        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                if (itemsFromClientToHost.ContainsKey(i))
                {
                    itemsFromClientToHost[i] += amount;
                    if (GameManager.instance.isHost)
                        display.SetReceivedAmount(i, itemsFromClientToHost[i]);
                    else
                        display.SetSentAmount(i, itemsFromClientToHost[i]);

                    onOverallChangedCallback?.Invoke(1);
                }
                break;
            }
        }
    }

    public void OverallCount(int taskId)
    {
        OverallCountServerRpc(taskId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OverallCountServerRpc(int taskId)
    {
        OverallCountClientRpc(taskId);
    }

    [ClientRpc]
    public void OverallCountClientRpc(int taskId)
    {
        switch (taskId)
        {
            case 0: // spawnerDestroyCount
                spawnerDestroyCount++;
                onOverallChangedCallback?.Invoke(30);
                break;
            case 1: // monsterKillCount
                monsterKillCount++;
                onOverallChangedCallback?.Invoke(31);
                break;
            default:
                Debug.Log("Wrong task id");
                break;
        }
    }

    public void ReceivedCount(int taskId, int amount)
    {
        ReceivedCountServerRpc(taskId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReceivedCountServerRpc(int taskId, int amount)
    {
        // 0: 스포너, 1: 몬스터
        ReceivedCountClientRpc(taskId, amount);
    }

    [ClientRpc]
    public void ReceivedCountClientRpc(int taskId, int amount)
    {
        switch (taskId)
        {
            case 0:
                spawnerBountyReceived += amount;
                break;
            case 1:
                monsterBountyReceived += amount;
                break;
            default:
                Debug.Log("Wrong task id");
                break;
        }
    }

    public bool OverallSentCheck()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemsFromHostToClient[i] != 0)
                return true;
        }

        return false;
    }

    public bool OverallReceivedCheck()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemsFromClientToHost[i] != 0)
                return true;
        }

        return false;
    }

    public int OverallPurchasedItemCheck(Item item)
    {
        int amout = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                amout = purchasedItems[i];
            }
        }

        return amout;
    }

    public int OverallSoldItemCheck(Item item)
    {
        int amout = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                amout = soldItems[i];
            }
        }

        return amout;
    }

    public int OverallProdItemCheck(Item item)
    {
        int amout = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] == item)
            {
                amout = itemsProduction[i];
            }
        }

        return amout;
    }

    public OverallSaveData SaveData()
    {
        OverallSaveData data = new OverallSaveData();

        data.itemsProduction = new Dictionary<int, int>(itemsProduction);
        data.itemsConsumption = itemsConsumption;
        data.purchasedItems = purchasedItems;
        data.soldItems = soldItems;
        data.itemsFromHostToClient = itemsFromHostToClient;
        data.itemsFromClientToHost = itemsFromClientToHost;
        data.spawnerDestroyCount = spawnerDestroyCount;
        data.spawnerBountyReceived = spawnerBountyReceived;
        data.monsterKillCount = monsterKillCount;
        data.monsterBountyReceived = monsterBountyReceived;

        return data;
    }

    public void LoadData(OverallSaveData data)
    {
        itemsProduction = data.itemsProduction;
        itemsConsumption = data.itemsConsumption;
        purchasedItems = data.purchasedItems;
        soldItems = data.soldItems;
        itemsFromHostToClient = data.itemsFromHostToClient;
        itemsFromClientToHost = data.itemsFromClientToHost;
        spawnerDestroyCount = data.spawnerDestroyCount;
        spawnerBountyReceived = data.spawnerBountyReceived;
        monsterKillCount = data.monsterKillCount;
        monsterBountyReceived = data.monsterBountyReceived;

        DisplayRefresh();
    }

    void DisplayRefresh()
    {
        display = OverallDisplay.instance;

        foreach (var item in itemsProduction)
        {
            if (item.Value != 0)
                display.SetProdAmount(item.Key, item.Value);
        }
        foreach (var item in itemsConsumption)
        {
            if (item.Value != 0)
                display.SetConsumptionAmount(item.Key, item.Value);
        }
        foreach (var item in purchasedItems)
        {
            if (item.Value != 0)
                display.SetPurchasedAmount(item.Key, item.Value);
        }
        foreach (var item in soldItems)
        {
            if (item.Value != 0)
                display.SetSoldAmount(item.Key, item.Value);
        }
        foreach (var item in itemsFromHostToClient)
        {
            if (item.Value != 0)
            {
                if (GameManager.instance.isHost)
                    display.SetSentAmount(item.Key, item.Value);
                else
                    display.SetReceivedAmount(item.Key, item.Value);
            }
        }
        foreach (var item in itemsFromClientToHost)
        {
            if (item.Value != 0)
            {
                if (GameManager.instance.isHost)
                    display.SetReceivedAmount(item.Key, item.Value);
                else
                    display.SetSentAmount(item.Key, item.Value);
            }
        }
    }
}
