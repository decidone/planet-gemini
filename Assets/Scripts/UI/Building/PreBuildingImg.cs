using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreBuildingImg : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public List<GameObject> buildingPosUnit = new List<GameObject>();
    [SerializeField]
    SpriteRenderer territoryView;
    [SerializeField]
    Sprite[] sprites;
    [SerializeField]
    Material[] materials;
    [SerializeField]
    BoxCollider2D boxCollider;
    public bool isEnergyUse;
    public Structure structure;

    public void PreStrSet(Structure str)
    {
        structure = str;
    }

    public void PreSpriteSet(Sprite _sprite)
    {
        spriteRenderer.sprite = _sprite;
    }

    public void PreAnimatorSet(RuntimeAnimatorController animatorController)
    {
        animator.runtimeAnimatorController = animatorController;
    }

    public void AnimSetFloat(string _string, int _int)
    {
        animator.SetFloat(_string, _int);
    }

    public void BoxColliderSet(Vector2 size)
    {
        boxCollider.size = size;
    }

    public bool CanPlaceBuilding(Vector2 size)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, size - (size / 10), 0f);


        foreach (var col in colliders)
        {
            if (col.GetComponent<UnitCommonAi>() || (col.GetComponent<PlayerController>() && col.isTrigger == false))
            {
                //Debug.Log(transform.position + " : " + (size - (size / 10)));
                return false; // 겹치는 유닛이 있음 → 배치 불가
            }
        }

        return true; // 아무것도 겹치지 않음 → 배치 가능
    }

    public void TerritoryViewSet(int index, Vector3 size, Color32 color)
    {
        territoryView.gameObject.SetActive(true);
        territoryView.transform.localScale = size;
        // index 0은 에너지, 2는 타워 범위
        territoryView.material = materials[index];
        territoryView.sprite = sprites[index];
        territoryView.color = color;
    }

    public void EnergyUseCheck(bool use)
    {
        isEnergyUse = use;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || (collision.GetComponent<PlayerController>() && collision.isTrigger == false))
        {
            if (!buildingPosUnit.Contains(collision.gameObject))
                buildingPosUnit.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || (collision.GetComponent<PlayerController>() && collision.isTrigger == false))
        {
            buildingPosUnit.Remove(collision.gameObject);
        }
    }
}
