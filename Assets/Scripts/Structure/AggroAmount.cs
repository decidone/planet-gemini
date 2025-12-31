using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroAmount : MonoBehaviour
{   
    public float baseAggroAmount = 0f;
    [SerializeField]
    float aggroAmount = 0f;
    float maxAggroAmount = 20;
    float aggroAmountPercent = 0.05f;
    bool isAggroActive = false;
    float aggroDecayStep = 1f;
    float aggroDecayInterval = 4f;

    public void SetAggroAmount(float damage, float attackSpeed)
    {
        float speedPer = (attackSpeed * 2) / 10;
        aggroAmount += (damage * aggroAmountPercent) + speedPer;
        
        if(aggroAmount > maxAggroAmount)
            aggroAmount = maxAggroAmount;

        if (!isAggroActive)
            StartCoroutine(AggroDecayTimer());
    }

    IEnumerator AggroDecayTimer()
    {
        isAggroActive = true;

        while (aggroAmount > 0)
        {
            yield return new WaitForSeconds(aggroDecayInterval);
            aggroAmount -= aggroDecayStep;
        }

        aggroAmount = 0;
        isAggroActive = false;
    }

    public float GetAggroAmount()
    {
        return baseAggroAmount + aggroAmount;
    }
}
