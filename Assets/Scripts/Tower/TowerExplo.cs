using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
public class TowerExplo : MonoBehaviour
{
    [SerializeField]
    protected Animator animator;

    void FxEnd(string str)
    {
        if (str == "false")
        {
            Destroy(this.gameObject, 0.1f);
        }
    }
}
