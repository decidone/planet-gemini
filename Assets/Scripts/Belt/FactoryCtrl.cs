using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FactoryCtrl : MonoBehaviour
{
    public bool isFull = false;
    public bool fluidIsFull = false;

    public int dirNum = 0;
    public int dirCount = 0;

    public bool isPreBuilding = false;
    public bool isSetBuildingOk = false;

    [SerializeField]
    protected GameObject unitCanvers = null;

    // HpBar ����
    [SerializeField]
    protected Image hpBar;
    protected float hp = 200.0f;
    [HideInInspector]
    public bool isRuin = false;

    // Repair ����
    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    protected virtual void SetDirNum() { } 
    // �ǹ��� ���� ����
    protected virtual void CheckPos() { }
    // ��ó ������Ʈ ���� ��ġ(�����¿�) ����
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback) { }
    // CheckPos�� ���� �������� ������Ʈ ����
    public virtual void DisableColliders() { }
    // �ݶ��̴� ����
    public virtual void EnableColliders() { }
    // �ݶ��̴� Ű��
    public virtual void SetBuild() { }
    // �ǹ� ��ġ ���

    public virtual void TakeDamage(float damage) { }
    protected virtual void DieFunc() { }
    public virtual void HealFunc(float heal) { }
    public virtual void RepairSet(bool repair) { }
    protected virtual void RepairFunc(bool isBuilding) { }
    protected virtual void RepairEnd() { }
}
