using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// UTF-8 설정
public class PlayerStatus : NetworkBehaviour
{
    public Image hpBar;
    public float hp = 100.0f;
    float maxHP = 100.0f;

    void Start()
    {
        hpBar.fillAmount = hp / maxHP;
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
        hpBar.fillAmount = hp / maxHP;
        if (hp <= 0f)
        {
            hp = 0f;
        }
    }
}
