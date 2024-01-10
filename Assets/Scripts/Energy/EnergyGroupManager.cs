using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGroupManager : MonoBehaviour
{
    #region Memo
    /*
     * 에너지 그룹 관리
     * 2중 리스트 - 에너지 그룹 번호/해당 그룹에 속한 에너지 관련 건물들로 구성
     *    - 변경: 그룹만 관리하고 구성원은 따로 관리 없이 각자 그룹번호를 가짐
     * 새로운 에너지건물 건설 및 기존 에너지건물 철거, 파괴 시 그룹 재산정 과정이 필요
     * 
     * 아래 기능들은 EnergyProvider(가칭)에서 어느정도 관리할 수 있음
     * 1. 새로운 건물이 지어질 때
     *    1. 기존 그룹과 이어지지 않고 새로운 그룹을 만들 때
     *    2. 기존 그룹에 추가될 때
     *    3. 기존 2개 이상의 그룹을 연결할 때
     * 2. 기존 건물이 철거, 파괴될 때
     *    1. 해당 그룹의 마지막 건물이 없어질 때
     *    2. 해당 건물이 없어져도 그룹에 변동이 없을 때
     *    3. 해당 건물이 없어져서 그룹이 분단될 때
     *    비고. 철거와 파괴는 다른 판정이긴 하나 에너지 관리 차원에서는 같게 취급함
    */
    #endregion

    public float syncFrequency;
    List<EnergyGroup> energyGroups;

    #region Singleton
    public static EnergyGroupManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of EnergyGroupManager found!");
            return;
        }

        instance = this;
    }
    #endregion

    void Start()
    {
        energyGroups = new List<EnergyGroup>();
        StartCoroutine(nameof(CalculateGroupsEnergy));
    }

    public void AddGroup(EnergyGroup group)
    {
        if (!energyGroups.Contains(group))
        {
            energyGroups.Add(group);
        }
        Debug.Log("add group: " + energyGroups.Count);
    }

    public void RemoveGroup(EnergyGroup group)
    {
        if (energyGroups.Contains(group))
        {
            energyGroups.Remove(group);
        }
        Debug.Log("subtract group: " + energyGroups.Count);
    }

    public void CheckGroups()
    {
        for (int i = 0; i < energyGroups.Count; i++)
        {
            EnergyGroup group = energyGroups[i];
            Debug.Log(i + " group energy: " + group.energy + ", consumption: " + group.consumption
                + ", efficiency: " + group.efficiency);
        }
    }

    public IEnumerator CalculateGroupsEnergy()
    {
        while (true)
        {
            for (int i =0; i < energyGroups.Count; i++)
            {
                energyGroups[i].EnergyCheck();
            }

            yield return new WaitForSeconds(syncFrequency);
        }
    }
}
