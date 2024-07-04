using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// UTF-8 설정
public class PlayerStatus : NetworkBehaviour
{
    public new string name;
    public Image hpBar;
    public float hp = 100.0f;
    public float maxHp = 100.0f;

    public delegate void OnHpChanged();
    public OnHpChanged onHpChangedCallback;

    void Start()
    {
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
        if (hp <= 0f)
            return;

        hp -= damage;
        if (hp < 0f)
            hp = 0f;
        onHpChangedCallback?.Invoke();
        hpBar.fillAmount = hp / maxHp;
    }
}
