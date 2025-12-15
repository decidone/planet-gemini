using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class MapObject : MonoBehaviour
{
    public new string name;
    public bool isInHostmap;
    public int objNum;

    public void RemoveMapObjRequest()
    {
        GameManager.instance.RemoveMapObjServerRpc(this.transform.position, isInHostmap);
        InfoUI.instance.SetDefault();
    }

    public void RemoveMapObj()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 1. GraphUpdateObject 생성
            GraphUpdateObject guo = new GraphUpdateObject(col.bounds);
            guo.modifyWalkability = true;
            guo.setWalkability = true;  // 통행 가능하게 설정

            // 2. 그래프 업데이트
            AstarPath.active.UpdateGraphs(guo);
        }

        Destroy(this.gameObject);
    }
}
