using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
