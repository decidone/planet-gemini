using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// UTF-8 설정
public class PlayerStatus : NetworkBehaviour
{
    public new string name;
    PlayerController playerController;
    [SerializeField]
    Image hpBar;
    [SerializeField]
    Image hpBackBar;
    public float hp = 100.0f;
    public float maxHp = 100.0f;
    public float tankHp;
    public float tankMaxHp;
    public float selfHealingAmount;
    public float selfHealInterval;
    float selfHealTimer;

    public bool isPlayerInHostMap = true;
    public bool isPlayerInMarket = false;

    public bool tankOn;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    void Awake()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        hpBar.fillAmount = hp / maxHp;
        HPUISet(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            if (NetworkObject.IsLocalPlayer)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }
    }

    private void OnDisable()
    {
        if (IsServer)
        {
            if (NetworkObject.IsLocalPlayer)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }

    #region ClientConnected
    protected virtual void OnClientConnectedCallback(ulong clientId)
    {
        ClientConnectSyncServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void ClientConnectSyncServerRpc()
    {
        if (tankOn)
            ClientConnectSyncClientRpc(hp, tankOn, tankHp, tankMaxHp, playerController.onTankData.NetworkObject);
        else
            ClientConnectSyncClientRpc(hp);
    }

    [ClientRpc]
    void ClientConnectSyncClientRpc(float hpSync)
    {
        hp = hpSync;
    }

    [ClientRpc]
    void ClientConnectSyncClientRpc(float hpSync, bool tankOnSync, float tankHpSync, float tankMaxHpSync, NetworkObjectReference networkObjectReference)
    {
        hp = hpSync;
        if (tankOnSync)
        {
            tankOn = tankOnSync;
            tankHp = tankHpSync;
            tankMaxHp = tankMaxHpSync;
            networkObjectReference.TryGet(out NetworkObject networkObject);
            playerController.onTankData = networkObject.GetComponent<TankCtrl>();
            playerController.tankOn = tankOnSync;
            Debug.Log(tankOnSync + " : " + networkObject.name);
        }
    }
    #endregion

    #region ClientDisconnected
    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObject != null)
            {
                var playerCt = playerObject.GetComponent<PlayerController>();
                if (playerCt != null)
                {
                    if (playerCt.onTankData)
                    {
                        playerCt.onTankData.ClientUISet();
                    }
                    playerCt.ClientDisConnServerRpc();
                }
            }
        }
    }
    #endregion

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!IsOwner) { return; }

        if (hp != maxHp)
        {
            selfHealTimer += Time.deltaTime;

            if (selfHealTimer >= selfHealInterval)
            {
                SelfHealingServerRpc();
                selfHealTimer = 0f;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SelfHealingServerRpc()
    {
        SelfHealingClientRpc();
    }

    [ClientRpc]
    void SelfHealingClientRpc()
    {
        hp += selfHealingAmount;

        if (hp >= maxHp)
        {
            hp = maxHp;
            HPUISet(false);
        }
        onHpChangedCallback?.Invoke();
        if (!tankOn)
        {
            hpBar.fillAmount = hp / maxHp;
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    public void TakeDamageClientRpc(float damage)
    {
        HPUISet(true);

        if (!tankOn)
        {
            if (hp <= 0f)
                return;

            hp -= damage;
            if (hp < 0f)
                hp = 0f;
            onHpChangedCallback?.Invoke();
            hpBar.fillAmount = hp / maxHp;
        }
        else
        {
            if (tankHp <= 0f)
                return;

            tankHp -= damage;
            if (tankHp < 0f)
            {
                tankHp = 0f;
                playerController.onTankData.CloseUI();
                if (IsServer)
                    TankDestoryServerRpc();
                return;
            }
            onHpChangedCallback?.Invoke();
            hpBar.fillAmount = tankHp / tankMaxHp;
        }
    }

    public void HPUISet(bool isOn)
    {
        if (isOn)
        {
            hpBar.enabled = true;
            hpBackBar.enabled = true;
        }
        else
        {
            hpBar.enabled = false;
            hpBackBar.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TankOnServerRpc(NetworkObjectReference networkObjectReference)
    {
        TankOnClientRpc(networkObjectReference);
    }

    [ClientRpc]
    void TankOnClientRpc(NetworkObjectReference networkObjectReference)
    {
        tankOn = true;
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.TryGetComponent(out TankCtrl tankCtrl);
        tankHp = tankCtrl.hp;
        tankMaxHp = tankCtrl.maxHp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TankOffServerRpc()
    {
        TankOffClientRpc();
    }

    [ClientRpc]
    void TankOffClientRpc()
    {
        tankOn = false;
        hpBar.fillAmount = hp / maxHp;
    }

    [ServerRpc(RequireOwnership = false)]
    void TankDestoryServerRpc()
    {
        TankDestoryClientRpc();
        playerController.TankDestoryServerRpc();
    }

    [ClientRpc]
    void TankDestoryClientRpc()
    {
        tankOn = false;
    }

    public void LoadGame()
    {
        hp = GameManager.instance.playerDataHp;

        if (tankOn)
        {
            hpBar.fillAmount = tankHp / tankMaxHp;
            HPUISet(tankHp < tankMaxHp);
        }
        else
        {
            hpBar.fillAmount = hp / maxHp;
            HPUISet(hp < maxHp);
        }
    }
}
