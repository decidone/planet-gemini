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

    // HpBar 관련
    [SerializeField]
    protected Image hpBar;
    protected float hp = 200.0f;
    [HideInInspector]
    public bool isRuin = false;

    // Repair 관련
    [HideInInspector]
    public bool isRepair = false;
    [SerializeField]
    protected Image repairBar;
    protected float repairGauge = 0.0f;

    protected virtual void SetDirNum() { } 
    // 건물의 방향 설정
    protected virtual void CheckPos() { }
    // 근처 오브젝트 찻는 위치(상하좌우) 설정
    protected virtual void CheckNearObj(Vector2 direction, int index, Action<GameObject> callback) { }
    // CheckPos의 방향 기준으로 오브젝트 찻기
    public virtual void DisableColliders() { }
    // 콜라이더 끄기
    public virtual void EnableColliders() { }
    // 콜라이더 키기
    public virtual void SetBuild() { }
    // 건물 설치 기능

    public virtual void TakeDamage(float damage) { }
    protected virtual void DieFunc() { }
    public virtual void HealFunc(float heal) { }
    public virtual void RepairSet(bool repair) { }
    protected virtual void RepairFunc(bool isBuilding) { }
    protected virtual void RepairEnd() { }
}
