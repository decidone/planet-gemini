using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TankCtrl : UnitAi
{
    public bool playerOnTank;
    [SerializeField]
    Image reloadingBar;
    [SerializeField]
    Image reloadingBackBar;
    public bool reloading;
    public float reloadTimer;
    public float reloadInterval;

    GameManager gameManager;
    [SerializeField]
    protected GameObject ui;
    [SerializeField]
    protected StructureInvenManager sInvenManager;
    public Inventory inventory;
    [SerializeField]
    protected RecipeManager rManager;
    List<Item> bulletRecipe = new List<Item>();
    List<Item> fuelItems = new List<Item>();
    protected Dictionary<string, Item> itemDic;
    GameObject canvas;
    public bool tankUIOpen;

    float maxFuel;
    public float fuel;
    float fuelConsumptionRate = 5f;

    protected override void Awake()
    {
        base.Awake();
        inventory = this.GetComponent<Inventory>();
    }

    protected override void Start()
    {
        base.Start();
        maxFuel = 100;
        gameManager = GameManager.instance;
        canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        sInvenManager = canvas.GetComponent<StructureInvenManager>();
        rManager = canvas.GetComponent<RecipeManager>();
        GetUIFunc();

        itemDic = ItemList.instance.itemDic;
        List<Recipe> recipes = rManager.GetRecipeList("TankInven");
        bulletRecipe = new List<Item>();

        foreach (Recipe recipeData in recipes)
        {
            if (recipeData.name == "Tank")
            {
                foreach (string itemsName in recipeData.items)
                {
                    bulletRecipe.Add(itemDic[itemsName]);
                }
            }
            else if (recipeData.name == "TankFuel")
            {
                foreach (string itemsName in recipeData.items)
                {
                    fuelItems.Add(itemDic[itemsName]);
                }
            }
        }
    }

    protected override void Update()
    {
        if (reloading)
        {
            reloadTimer += Time.deltaTime;
            reloadingBar.fillAmount = reloadTimer / reloadInterval;

            if (reloadTimer >= reloadInterval)
            {
                reloading = false;
                ReloadingUISet(false);
                if (hp == maxHp)
                {
                    unitCanvas.SetActive(false);
                }
            }
        }

        if (!IsServer)
            return;

        if (hp != maxHp && aIState != AIState.AI_Die)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                SelfHealingServerRpc();
                selfHealTimer = 0f;
            }
        }
    }

    protected override void FixedUpdate() { }

    public void FillFuel()
    {
        var slot = inventory.SlotCheck(1);
        if (fuel <= maxFuel / 2 && slot.amount > 0)
        {
            inventory.SlotSubServerRpc(1, 1);
            Overall.instance.OverallConsumption(slot.item, 1);
            fuel += maxFuel / 2;
            soundManager.PlaySFX(gameObject, "structureSFX", "Flames");
        }
    }

    public void TankMove()
    {
        fuel -= fuelConsumptionRate * Time.deltaTime;
        if (fuel <= 0)
        {
            fuel = 0;
        }
    }

    public float GetProgress() { return fuel; }

    protected override void OnClientConnectedCallback(ulong clientId)
    {
        base.OnClientConnectedCallback(clientId);
        if (inventory != null)
            ItemSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ItemSyncServerRpc()
    {
        ItemListClearClientRpc();
        for (int i = 0; i < inventory.space; i++)
        {
            var slot = inventory.SlotCheck(i);

            int itemIndex = GeminiNetworkManager.instance.GetItemSOIndex(slot.item);
            if (itemIndex != -1)
                ItemSyncClientRpc(i, itemIndex, slot.amount);
        }
    }


    [ClientRpc]
    protected void ItemListClearClientRpc()
    {
        if (!IsServer)
            inventory.ResetInven();
    }


    [ClientRpc]
    protected void ItemSyncClientRpc(int slotNum, int itemIndex, int itemAmount, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
            return;

        Item item = GeminiNetworkManager.instance.GetItemSOFromIndex(itemIndex);
        inventory.RecipeSlotAdd(slotNum, item, itemAmount);
    }


    [ServerRpc(RequireOwnership = false)]
    public override void ClientConnectSyncServerRpc()
    {
        base.ClientConnectSyncServerRpc();
        ClientConnectSyncClientRpc(hp, fuel);
        if (playerOnTank)
        {
            PlayerTankOnClientRpc();
        }
    }

    [ClientRpc]
    void ClientConnectSyncClientRpc(float hpSync, float fuelSync)
    {
        hp = hpSync;
        maxFuel = 100;
        fuel = fuelSync;
    }

    [ClientRpc]
    public void PlayerTankOnClientRpc()
    {
        playerOnTank = true;
        transform.position = new Vector3(-100, -100, 0);
        gameObject.SetActive(false);
    }

    public void PlayerTankOff(Vector3 pos, float setHp, bool reload, float timer, float interval)
    {
        playerOnTank = false;
        hp = setHp;

        if (reload)
        {
            reloading = reload;
            reloadTimer = timer;
            reloadInterval = interval;
            unitCanvas.SetActive(true);
            ReloadingUISet(true);
        }
        transform.position = pos;
        gameObject.SetActive(true);
    }

    void ReloadingUISet(bool isOn)
    {
        if (isOn)
        {
            reloadingBar.enabled = true;
            reloadingBackBar.enabled = true;
        }
        else
        {
            reloadingBar.enabled = false;
            reloadingBackBar.enabled = false;
        }
    }

    public void TankDestory()
    {
        if (IsServer)
            DieFuncServerRpc();
    }

    public void PlayerOnTankLoad(float tankHp, float tankMaxHp)
    {
        unitIndex = 3;
        hp = tankHp;
        maxHp = tankMaxHp;
        playerOnTank = true;
    }

    public override void GameStartSet(UnitSaveData unitSave)
    {
        unitIndex = unitSave.unitIndex;
        if (inventory != null)
            inventory.LoadData(unitSave.inven);
        hp = unitSave.hp;
        fuel = unitSave.fuel;

        if (hp < maxHp)
        {
            hpBar.fillAmount = hp / maxHp;
            unitCanvas.SetActive(true);
        }
    }

    public override UnitSaveData SaveData()
    {
        UnitSaveData data = new UnitSaveData();

        data.unitIndex = GeminiNetworkManager.instance.GetMonsterSOIndex(this.gameObject, 0, true);
        data.hp = hp;
        data.pos = Vector3Extensions.FromVector3(transform.position);
        data.playerOnTank = playerOnTank;
        data.fuel = fuel;

        if (TryGetComponent(out Inventory inventory))
        {
            data.inven = inventory.SaveData();
        }

        return data;
    }

    void GetUIFunc()
    {
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.StructureStorageArr)
        {
            if (list.name == "Tank")
            {
                ui = list;
            }
        }
    }

    public bool TankAttackCheck()
    {
        bool canAttack = false;

        var bulletCheck = inventory.SlotCheck(0);
        if (bulletCheck.amount > 0)
        {
            canAttack = true;
        }

        return canAttack;
    }

    public void OpenUI()
    {
        GameObject canvas = gameManager.GetComponent<GameManager>().inventoryUiCanvas;
        InventoryList inventoryList = canvas.GetComponent<InventoryList>();

        foreach (GameObject list in inventoryList.InventoryArr)
        {
            if (list.name == "StructureInfo")
            {
                Button closeBtn = list.transform.Find("CloseButton").gameObject.GetComponent<Button>();
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(CloseUI);
            }
        }

        gameManager.TankUIOpen();
        sInvenManager.SetInven(inventory, ui);
        sInvenManager.SetTank(this);
        sInvenManager.progressBar.SetMaxProgress(maxFuel);

        sInvenManager.slots[0].SetInputItem(bulletRecipe);
        sInvenManager.slots[1].SetInputItem(fuelItems);
        sInvenManager.OpenUI();
        tankUIOpen = true;
    }

    public void ClientUISet()
    {
        ClientUISetServerRpc(fuel);
        Debug.Log("ClientUISet" + fuel);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void ClientUISetServerRpc(float syncFuel)
    {
        ClientUISetClientRpc(syncFuel);
    }

    [ClientRpc]
    protected void ClientUISetClientRpc(float syncFuel)
    {
        fuel = syncFuel;
        Debug.Log("ClientUISetClientRpc" + fuel);
    }

    public void CloseUI()
    {
        if (tankUIOpen)
        {
            sInvenManager.ReleaseInven();
            sInvenManager.CloseUI();
            tankUIOpen = false;
        }
    }
}
