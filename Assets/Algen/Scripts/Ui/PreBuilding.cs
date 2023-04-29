using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PreBuilding : MonoBehaviour
{
    public static PreBuilding instance;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    GameObject gameObj = null; 
    public bool isSelect = false;

    Collider[] colliders;

    public GameObject beltMgr = null;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of drag slot found!");
            return;
        }

        instance = this;        
    }

    void Update()
    {
        InputCheck();

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotationImg();
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);        
    }

    protected virtual void InputCheck()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {//해당 좌표에 오브젝트가 있음 설치가 안되도록 수정해야함
            if (gameObj != null)
            {
                GameObject obj = Instantiate(gameObj);
                SetSlotColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
                obj.transform.position = this.transform.position;

                if(obj.TryGetComponent(out FactoryCtrl factory))
                {
                    factory.isPreBuilding = false; 
                }
                else if (obj.TryGetComponent(out TowerAi tower))
                {
                    tower.isPreBuilding = false;
                    tower.EnableColliders();
                }
                else if (obj.TryGetComponent(out BeltGroupMgr belt))
                {
                    obj.transform.parent = beltMgr.transform;
                    belt.isPreBuilding = false;
                    belt.BeltList[0].isPreBuilding = false;
                }
                BuildingInfo.instance.BuildingEnd();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ReSetImage();
        }
    }

    public void SetImage(GameObject game)
    {
        if (this.transform.childCount > 0)
        {
            GameObject temp = transform.GetChild(0).gameObject;
            Destroy(temp);
        }

        gameObj = Instantiate(game);

        if (gameObj.TryGetComponent(out FactoryCtrl factory))
        {
            factory.isPreBuilding = true;
        }
        else if (gameObj.TryGetComponent(out TowerAi tower))
        {
            tower.isPreBuilding = true;
            tower.DisableColliders();
        }
        else if (gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.isPreBuilding = true;
            belt.SetBelt(0);
            belt.BeltList[0].isPreBuilding = true;
        }

        gameObj.transform.position = this.transform.position;
        gameObj.transform.parent = this.transform;

        spriteRenderer = gameObj.GetComponentInChildren<SpriteRenderer>();
        SetSlotColor(spriteRenderer, Color.green, 0.35f);

        isSelect = true;
    }

    void SetSlotColor(SpriteRenderer sprite, Color color, float alpha)
    {
        Color slotColor = sprite.color;
        slotColor = color;
        slotColor.a = alpha;
        sprite.color = slotColor;
    }

    public void ReSetImage()
    {
        if (this.transform.childCount > 0)
        {
            GameObject temp = transform.GetChild(0).gameObject;
            Destroy(temp);
        }

        if(spriteRenderer)
            spriteRenderer.sprite = null;
        isSelect = false;
        gameObj = null;
        gameObject.SetActive(false);
    }

    void RotationImg()
    {
        if (gameObj.TryGetComponent(out FactoryCtrl factory))
        {
            factory.dirNum++;
            if(factory.dirNum >= factory.dirCount)
                factory.dirNum = 0;
            //spriteRenderer.sprite = gameObj.GetComponent<Sprite>();
        }
        else if(gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.BeltList[0].dirNum++;
            if (belt.BeltList[0].dirNum >= belt.BeltList[0].dirCount)
                belt.BeltList[0].dirNum = 0;
        }
    }
}
