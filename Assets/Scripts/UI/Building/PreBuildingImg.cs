using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreBuildingImg : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public List<GameObject> buildingPosUnit = new List<GameObject>();
    [SerializeField]
    GameObject territoryView;
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
                Debug.Log(transform.position + " : " + (size - (size / 10)));
                return false; // 겹치는 유닛이 있음 → 배치 불가
            }
        }

        return true; // 아무것도 겹치지 않음 → 배치 가능
    }

    public void TerritoryViewSet(int index)
    {
        territoryView.SetActive(true);
        Color32 newColor;
        SpriteRenderer spriteRenderer = territoryView.GetComponent<SpriteRenderer>();
        switch (index)
        {
            case 0: //ImprovedRepeater
                territoryView.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                spriteRenderer.material = materials[0];
                spriteRenderer.sprite = sprites[0];
                break;
            case 1: //Normal Energy Str
                spriteRenderer.material = materials[0];
                spriteRenderer.sprite = sprites[0];
                break;
            case 2: //Overclock
                territoryView.transform.localScale = new Vector3(1.95f, 1.95f, 1f);
                spriteRenderer.material = materials[1];
                spriteRenderer.sprite = sprites[1];
                newColor = new Color32(0, 255, 158, 100);
                spriteRenderer.color = newColor;
                break;
            case 3: //RepairTower
                territoryView.transform.localScale = new Vector3(1.56f, 1.56f, 1f);
                spriteRenderer.material = materials[1];
                spriteRenderer.sprite = sprites[1];
                newColor = new Color32(45, 70, 195, 100);
                spriteRenderer.color = newColor;
                break;
            case 4: //SunTower
                territoryView.transform.localScale = new Vector3(3.12f, 3.12f, 1f);
                spriteRenderer.material = materials[1];
                spriteRenderer.sprite = sprites[1];
                newColor = new Color32(148, 0, 211, 100);
                spriteRenderer.color = newColor;
                break;
            case 5: //AttackTower
                territoryView.transform.localScale = new Vector3(2f, 2f, 2f);
                spriteRenderer.material = materials[1];
                spriteRenderer.sprite = sprites[1];
                newColor = new Color32(255, 0, 0, 100);
                spriteRenderer.color = newColor;
                break;
        }
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
