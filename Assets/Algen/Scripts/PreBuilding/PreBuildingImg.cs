using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreBuildingImg : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    [HideInInspector]
    public bool canBuilding;
    protected List<GameObject> buildingPosUnit = new List<GameObject>();

    private void Start()
    {
        canBuilding = true;
    }

    public void PreSpriteSet(Sprite _sprite)
    {
        spriteRenderer.sprite = _sprite;
    }

    public void PreAnimatorSet(RuntimeAnimatorController animatorController)
    {
        animator.runtimeAnimatorController = animatorController;
    }

    public void AnimSetBool(string _string, bool _bool)
    {
        animator.SetBool(_string, _bool);
    }

    public void AnimSetFloat(string _string, int _int)
    {
        animator.SetFloat(_string, _int);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || collision.GetComponent<PlayerController>())
        {
            buildingPosUnit.Add(collision.gameObject);

            if (buildingPosUnit.Count > 0)
            {
                if (!collision.GetComponentInParent<PreBuilding>())
                {
                    canBuilding = false;
                }

                PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                if (preBuilding != null)
                {
                    preBuilding.isBuildingOk = false;
                }
            }            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || collision.GetComponent<PlayerController>())
        {
            buildingPosUnit.Remove(collision.gameObject);
            if (buildingPosUnit.Count > 0)
                canBuilding = false;
            else
            {
                canBuilding = true;

                PreBuilding preBuilding = GetComponentInParent<PreBuilding>();
                if (preBuilding != null)
                    preBuilding.isBuildingOk = true;
            }            
        }
    }
}
