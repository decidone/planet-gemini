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
    public float selfHealingAmount;
    public float selfHealInterval;
    float selfHealTimer;

    public bool isPlayerInHostMap = true;
    public bool isPlayerInMarket = false;

    bool tankOn;
    TankCtrl tankData;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    void Awake()
    {
        HPUISet(false);
    }

    void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        hpBar.fillAmount = hp / maxHp;
    }

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
        hpBar.fillAmount = hp / maxHp;
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
        else if(tankData != null)
        {
            if (tankData.hp <= 0f)
                return;

            tankData.hp -= damage;
            if (tankData.hp < 0f)
            {
                tankData.hp = 0f;
                TankDestory();
                return;
            }
            onHpChangedCallback?.Invoke();
            hpBar.fillAmount = tankData.hp / tankData.maxHp;
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

    public void TankOn(TankCtrl tankCtrl)
    {
        tankOn = true;
        tankData = tankCtrl;
    }

    public void TankOff()
    {
        tankOn = false;
    }

    void TankDestory()
    {
        tankData = null;
        tankOn = false;
        playerController.TankDestory();
    }

    public void LoadGame()
    {
        hp = GameManager.instance.playerDataHp;
        HPUISet(hp >= maxHp);
    }
}
