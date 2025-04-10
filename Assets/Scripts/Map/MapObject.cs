using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class MapObject : MonoBehaviour
{
    public new string name;
    public bool isInHostmap;

    public void RemoveMapObjRequest()
    {
        // 세이브 데이터 불러올 때 지워진 오브젝트 체크 가능하도록 어딘가에 저장하는거 추가할 것
        GameManager.instance.RemoveMapObjServerRpc(this.transform.position, isInHostmap);
        InfoUI.instance.SetDefault();
    }

    public void RemoveMapObj()
    {
        Destroy(this.gameObject);
    }
}
