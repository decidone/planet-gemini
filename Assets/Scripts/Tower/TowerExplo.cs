using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerExplo : MonoBehaviour
{
    [SerializeField]
    protected Animator animator;
    //에니메이션 이벤트로 호출되는 함수
    void FxEnd(string str)
    {
        if (str == "false")
        {
            Destroy(this.gameObject, 0.1f);
        }
    }
}
