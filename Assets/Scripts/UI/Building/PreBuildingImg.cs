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
    Sprite[] sprites;
    [SerializeField]
    Material[] materials;

    public bool isEnergyUse;
    public Structure structure;

    private void Start()
    {
        canBuilding = true;
    }

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
        }
    }

    public void EnergyUseCheck(bool use)
    {
        isEnergyUse = use;
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
