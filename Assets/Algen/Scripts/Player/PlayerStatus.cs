using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{    
    // HpBar ����
    public Image hpBar;
    float hp = 100.0f;
    float maxHP = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        hpBar.fillAmount = hp / maxHP;
    }

    public void TakeDamage(float damage)
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