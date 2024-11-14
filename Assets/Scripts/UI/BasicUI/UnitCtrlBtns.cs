using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitCtrlBtns : MonoBehaviour
{
    PlayerController player;
    UnitDrag unitDrag;

    void Start()
    {
        unitDrag = GameManager.instance.GetComponent<UnitDrag>();
    }

    public void UnitAttackBtnFunc()
    {
        unitDrag.Attack();
    }

    public void UnitPatrolBtnFunc()
    {
        unitDrag.Patrol();
    }

    public void UnitHoldBtnFunc()
    {
        unitDrag.Hold();
    }

    public void TankInvenBtnFunc()
    {
        if (!player)
        {
            player = GameManager.instance.player.GetComponent<PlayerController>();
        }

        player.TankInven();
    }

    public void TankAttackBtnFunc()
    {
        if (!player)
        {
           player =  GameManager.instance.player.GetComponent<PlayerController>();
        }

        player.TankAttack();
    }
}
