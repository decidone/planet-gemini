using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchObjectsInRangeManager : MonoBehaviour
{
    List<UnitCommonAi> unitList = new List<UnitCommonAi>();
    List<TowerAi> towerList = new List<TowerAi>();
    List<Structure> structureList = new List<Structure>();

    int searchCap = 50;

    float unitSearchInterval = 0.3f;
    float unitSearchTimer = 0f;
    int unitCurrentIndex = 0;

    float towerSearchInterval = 0.9f;
    float towerSearchTimer = 0f;
    int towerCurrentIndex = 0;

    bool isUnitProcessing = false;
    bool isTowerProcessing = false;

    private HashSet<UnitCommonAi> pendingUnitRemove = new HashSet<UnitCommonAi>();
    private HashSet<TowerAi> pendingTowerRemove = new HashSet<TowerAi>();

    #region Singleton
    public static SearchObjectsInRangeManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (!isUnitProcessing)
        {
            unitSearchTimer += Time.deltaTime;

            if (unitSearchTimer >= unitSearchInterval)
            {
                unitSearchTimer = 0f;
                isUnitProcessing = true;
                unitCurrentIndex = 0;
            }

            if (pendingUnitRemove.Count > 0)
            {
                PendingUnitRemoveFunc();
            }
        }
        else
        {
            if (unitCurrentIndex < unitList.Count)
            {
                int remaining = unitList.Count - unitCurrentIndex;
                int count = Mathf.Min(searchCap, remaining);

                for (int i = 0; i < count; i++)
                {
                    UnitCommonAi unit = unitList[unitCurrentIndex];

                    if (unit)
                        unit.SearchObjectsInRange();
                    else
                        pendingUnitRemove.Add(unit);

                    unitCurrentIndex++;
                }

                // 전체를 다 돌았으면 사이클 종료
                if (unitCurrentIndex >= unitList.Count)
                {
                    isUnitProcessing = false;
                }
            }
            else
                isUnitProcessing = false;
        }

        if (!isTowerProcessing)
        {
            towerSearchTimer += Time.deltaTime;

            if (towerSearchTimer >= towerSearchInterval)
            {
                towerSearchTimer = 0f;
                isTowerProcessing = true;
                towerCurrentIndex = 0;
            }

            PendingTowerRemoveFunc();
        }
        else
        {
            if (towerCurrentIndex < towerList.Count)
            {
                int remaining = towerList.Count - towerCurrentIndex;
                int count = Mathf.Min(searchCap, remaining);

                for (int i = 0; i < count; i++)
                {
                    TowerAi tower = towerList[towerCurrentIndex];

                    if (tower)
                        tower.SearchObjectsInRange();
                    else
                        pendingTowerRemove.Add(tower);

                    towerCurrentIndex++;
                }

                // 전체를 다 돌았으면 사이클 종료
                if (towerCurrentIndex >= towerList.Count)
                {
                    isTowerProcessing = false;
                }
            }
            else
                isTowerProcessing = false;
        }
    }

    public void UnitListAdd(UnitCommonAi unit)
    {
        if (!unitList.Contains(unit))
            unitList.Add(unit);
    }

    public void UnitListRemove(UnitCommonAi unit)
    {
        pendingUnitRemove.Add(unit); // 안전한 타이밍에 업데이트에서 몰아서 제거
    }

    void PendingUnitRemoveFunc()
    {
        unitList.RemoveAll(u => u == null || pendingUnitRemove.Contains(u));
        pendingUnitRemove.Clear();
    }

    public void TowerListAdd(TowerAi tower)
    {
        if (!towerList.Contains(tower))
            towerList.Add(tower);
    }

    public void TowerListRemove(TowerAi tower)
    {
        pendingTowerRemove.Add(tower); // 안전한 타이밍에 업데이트에서 몰아서 제거
    }

    void PendingTowerRemoveFunc()
    {
        towerList.RemoveAll(t => t == null || pendingTowerRemove.Contains(t));
        pendingTowerRemove.Clear();
    }

    public void StructureListAdd(Structure str) // 오버클록, 리페어 타워용
    {
        if (!structureList.Contains(str))
            structureList.Add(str);
    }

    public void StructureListRemove(Structure str)
    {
        structureList.Remove(str);
    }

    public void StrSearchFunc()
    {
        StartCoroutine(StructureSearchCoroutine());
    }

    IEnumerator StructureSearchCoroutine()
    {
        yield return null;

        for (int i = 0; i < structureList.Count; i++)
        {
            Structure str = structureList[i];
            if (str)
                str.SearchObjectsInRange();

            if((i + 1) % searchCap == 0)
                yield return null;
        }
    }
}
