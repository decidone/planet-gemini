using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemFXCtrl : MonoBehaviour
{
    public Animator animator;
    Vector3 moveNextStep = Vector3.zero;    // ¿Ãµø πÊ«‚ ∫§≈Õ
    public Transform aggroTarget = null;   // ≈∏∞Ÿ
    int attackMotionNum;
    bool isAnimEnd = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {        
        AttackFunc();

    }

    public void FXEnd()
    {
        isAnimEnd = true;
    }

    void AttackFunc()
    {
        if (attackMotionNum == 0)
        {
            //this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            ImgMrror();
            if (isAnimEnd == true)
            {
                Destroy(this.gameObject, 0.1f);
            }
        }
        else if (attackMotionNum == 1)
        {
            if (isAnimEnd == true)
            {
                Destroy(this.gameObject, 0.1f);
            }
        }
        else if (attackMotionNum == 2)
        {
            transform.position += moveNextStep * 2 * Time.fixedDeltaTime;
            Destroy(this.gameObject, 3f);
        }
    }

    void FXMove()
    {
        
    }//void MonsterMove()
    void ImgMrror()
    {
        if (moveNextStep.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveNextStep.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }//void ImgMrror()

    public void GetTarget(Vector3 target, int attackMotion)
    {
        moveNextStep = (target - transform.position).normalized;
        //moveNextStep.Normalize();

        attackMotionNum = attackMotion;
    }

    public void CollOn()
    {
        //this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }
}
