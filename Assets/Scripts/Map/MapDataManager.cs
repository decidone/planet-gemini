using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDataManager : MonoBehaviour
{
    // 바이옴 리스트, 바이옴별 타일들, 등등 저장하거나 불러올 때 리스트를 참조해야 하는 것들
    // 굳이 인스펙터로 안 넣어도 맵젠쪽에 한번 요정해서 다 가져온 다음 리스트로 만들어두면 됨 그 쪽이 유지보수가 더 쉬울 듯

    [SerializeField] List<Biome> biomes;     // biome 스크립트의 biomeNum과 동일한 순번으로 저장되어있음. 0: plain, 1: forest, 2: desert, 3: snow, 4: frozen, 5: lake, 6: cliff
    [SerializeField] List<Resource> resources;   // 0: Coal, 1: Stone, 2: CopperOre, 3: IronOre, 4: AdamantiteOre, 5: CrudeOil
    [SerializeField] List<GameObject> mapObj;
    [SerializeField] List<string> buildable;
    List<List<Tile>> tiles = new();

    #region Singleton
    public static MapDataManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Init();
    }
    #endregion

    public void Init()
    {
        if (tiles.Count != 0) return;

        for (int i = 0; i < biomes.Count; i++)
        {
            tiles.Add(biomes[i].GetTilesList());
        }
    }

    public CellSaveData SaveCellData(Cell cell)
    {
        CellSaveData data = new CellSaveData();
        data.dataList[0] = cell.x;
        data.dataList[1] = cell.y;
        data.dataList[2] = cell.biome.biomeNum;
        for (int i = 0; i < tiles[data.dataList[2]].Count; i++)
        {
            if (cell.tile == tiles[data.dataList[2]][i])
            {
                data.dataList[3] = i;
            }
        }

        data.dataList[4] = -1;
        if (cell.exTile != null)
        {
            int count = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].Contains(cell.exTile))
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        if (cell.exTile == tiles[i][j])
                        {
                            data.dataList[4] = count;
                            break;
                        }
                        else
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    count += tiles[i].Count;
                }

                if (data.dataList[4] != -1)
                    break;
            }
        }

        data.dataList[5] = -1;
        for (int i = 0; i < resources.Count - 1; i++)
        {
            // oil 제외
            if (cell.resource == resources[i])
            {
                data.dataList[5] = i;
                break;
            }
        }
        // oil
        if (cell.resource == resources[resources.Count - 1])
        {
            data.dataList[5] = resources.Count - 1 + cell.oilTile;
        }

        if (cell.obj != null)
        {
            data.dataList[6] = cell.objNum;
        }
        else
        {
            data.dataList[6] = -1;
        }

        if (cell.buildable.Count > 0)
        {
            foreach (var item in cell.buildable)
            {
                for (int i = 0; i < buildable.Count; i++) {

                    if (item.Equals(buildable[i]))
                    {
                        data.buildable.Add(i);
                    }
                }
            }
        }

        return data;
    }

    public Cell LoadCellData(CellSaveData cellSaveData)
    {
        Cell cell = new Cell();
        cell.x = cellSaveData.dataList[0];
        cell.y = cellSaveData.dataList[1];
        cell.biome = biomes[cellSaveData.dataList[2]];
        cell.tile = tiles[cellSaveData.dataList[2]][cellSaveData.dataList[3]];
        if (cellSaveData.dataList[4] != -1)
        {
            int count = cellSaveData.dataList[4];
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].Count <= count)
                {
                    count -= tiles[i].Count;
                }
                else
                {
                    for (int j = 0; j < tiles[i].Count; j++)
                    {
                        if (count == 0)
                        {
                            cell.exTile = tiles[i][j];
                            break;
                        }
                        else
                        {
                            count--;
                        }
                    }
                }

                if (cell.exTile != null)
                    break;
            }
        }

        cell.resourceNum = cellSaveData.dataList[5];
        if (cellSaveData.dataList[5] != -1)
        {
            if (cellSaveData.dataList[5] < 5)
            {
                cell.resource = resources[cellSaveData.dataList[5]];
            }
            else
            {
                cell.resource = resources[5];
                cell.oilTile = cellSaveData.dataList[5] - 5;
            }
        }

        cell.objNum = cellSaveData.dataList[6];

        if (cellSaveData.buildable.Count > 0)
        {
            for (int i = 0; i < cellSaveData.buildable.Count; i++)
            {
                cell.buildable.Add(buildable[cellSaveData.buildable[i]]);
            }
        }

        return cell;
    }

    public GameObject GetMapObjByNum(int objNum)
    {
        return mapObj[objNum];
    }
}
