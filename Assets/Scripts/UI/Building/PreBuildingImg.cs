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
    [SerializeField]
    GameObject territoryView;
    [SerializeField]
    Material[] materials;

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

    public void AnimSetFloat(string _string, int _int)
    {
        animator.SetFloat(_string, _int);
    }

    public void TerritoryViewSet(int index)
    {
        territoryView.SetActive(true);
        switch (index)
        {
            case 1:
                territoryView.GetComponent<SpriteRenderer>().material = materials[0];
                break;
            case 2:
                territoryView.transform.localScale = new Vector3(1.95f, 1.95f, 1f);
                territoryView.GetComponent<SpriteRenderer>().material = materials[1];
                break;
            case 3:
                territoryView.transform.localScale = new Vector3(1.95f, 1.95f, 1f);
                territoryView.GetComponent<SpriteRenderer>().material = materials[2];
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<UnitCommonAi>() || collision.GetComponent<PlayerController>())
        {
            buildingPosUnit.Add(collision.gameObject);

            if (buildingPosUnit.Count > 0)
            {
                canBuilding = false;
                PreBuilding.instance.isBuildingOk = false;                
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
                PreBuilding.instance.isBuildingOk = true;
            }            
        }
    }
}
