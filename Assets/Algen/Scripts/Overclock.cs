using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overclock : Production
{
    public List<Structure> buildingList = new List<Structure>();
    protected float searchTimer = 1f;
    protected float searchInterval = 1f; // 딜레이 간격 설정
    [SerializeField]
    SpriteRenderer view;
    GameManager gameManager;
    PreBuilding preBuilding;
    bool preBuildingCheck;

    protected override void Start()
    {
        base.Start();
        gameManager = GameManager.instance;
        preBuilding = PreBuilding.instance;
    }

    protected override void Update()
    {
        base.Update();

        //if (gameManager.focusedStructure == null)
        //{
        //    if (preBuilding.isBuildingOn && !removeState)
        //    {
        //        if (!preBuildingCheck)
        //        {
        //            preBuildingCheck = true;
        //            view.enabled = true;
        //        }
        //    }
        //    else
        //    {
        //        if (preBuildingCheck)
        //        {
        //            preBuildingCheck = false;
        //            view.enabled = false;
        //        }
        //    }
        //}
        if (!isPreBuilding && IsServer)
        {
            searchTimer += Time.deltaTime;

            if (searchTimer >= searchInterval)
            {
                SearchObjectsInRange();
                searchTimer = 0f; // 탐색 후 타이머 초기화
            }
        }
    }

    private void SearchObjectsInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, structureData.ColliderRadius);

        foreach (Collider2D collider in colliders)
        {
            GameObject building = collider.gameObject;

            if (building.TryGetComponent(out Structure structure))
            {
                if (structure.overclockTower == null && !structure.isPreBuilding
                    && !buildingList.Contains(structure) && !structure.GetComponent<Portal>())
                {
                    buildingList.Add(structure);
                    structure.overclockTower = this;
                    if (conn != null && conn.group != null && conn.group.efficiency > 0)
                    {
                        structure.OverclockSet(true);
                    }
                }
            }
        }
    }

    public void RemoveObjectsOutOfRange(Structure obj)//근쳐 타워 삭제시 발동되게
    {
        if (buildingList.Contains(obj))
        {
            buildingList.Remove(obj);
        }
    }

    public override void AddConnector(EnergyGroupConnector connector)
    {
        base.AddConnector(connector);
        foreach (Structure building in buildingList)
        {
            building.OverclockSet(true);
        }
    }

    public override void RemoveConnector(EnergyGroupConnector connector)
    {
        base.RemoveConnector(connector);
        foreach (Structure building in buildingList)
        {
            building.OverclockSet(false);
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
