using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovePos : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Animator anim;

    #region Singleton
    public static UnitMovePos instance;

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

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    public void AnimStart(Vector2 pos)
    {
        transform.position = pos;
        spriteRenderer.enabled = true;
        anim.enabled = true;
        anim.Play("UnitMovePos", -1, 0f);
    }

    public void AnimEnd()
    {
        spriteRenderer.enabled = false;
        anim.enabled = false;
    }
}
