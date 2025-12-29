using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overclock : Production
{
    public List<Production> buildingList = new List<Production>();
    protected float searchTimer = 1f;
    protected float searchInterval = 1f; // 딜레이 간격 설정
    [SerializeField]
    SpriteRenderer view;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(EfficiencyCheckLoop());
    }

    protected override void Update()
    {
        base.Update();

        if (!isPreBuilding && IsServer)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }
        }

        if (conn != null && conn.group != null)
        {
            if(conn.group.efficiency > 0 && !isOperate)
                OverclockOn(true);
            else if(conn.group.efficiency == 0 && isOperate)
                OverclockOn(false);
        }
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject building = collider.gameObject;

            if (building.TryGetComponent(out Production production))
            {
                if (production.overclockTower == null && !production.isPreBuilding
                    && !buildingList.Contains(production) && !production.GetComponent<Portal>())
                {
                    buildingList.Add(production);
                    production.overclockTower = this;
                    if (conn != null && conn.group != null && conn.group.efficiency > 0)
                    {
                        production.OverclockSyncServerRpc(true);
                    }
                }
            }
        }
    }

    public void RemoveObjectsOutOfRange(Production obj) //근처 타워 삭제시 발동되게
    {
        if (buildingList.Contains(obj))
        {
            buildingList.Remove(obj);
        }
    }

    public void OverclockOn(bool isOn)
    {
        Debug.Log(isOn);
        foreach (Production building in buildingList)
        {
            building.OverclockSyncServerRpc(isOn);
        }
        isOperate = isOn;
    }

    public void OverclockRemove()
    {
        foreach (Production building in buildingList)
        {
            building.overclockTower = null;
            building.OverclockSyncServerRpc(false);
        }
    }

    public override Dictionary<Item, int> PopUpItemCheck() { return null; }

    public override void AddInvenItem() { }

    public override void SetBuild()
    {
        base.SetBuild();
        view.enabled = false;
    }

    public override void Focused()
    {
        view.enabled = true;
    }

    public override void DisableFocused()
    {
        view.enabled = false;
    }
}
