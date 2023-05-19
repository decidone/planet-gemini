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

    public GameObject beltMgr = null;

    Vector3 setPos = new Vector3(-0.5f, -0.5f);
    Vector2 boxSize;
    public Vector2Int size; // 건물의 크기

    //bool isBuildingOk = false;

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
        Vector3 roundedPosition = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y) , 0f);
        transform.position = new Vector3(roundedPosition.x, roundedPosition.y, transform.position.z);

        //if (gameObj != null) // 오브젝트 중첩설치 안되도록
        //{
        //    Collider2D[] colliders = Physics2D.OverlapBoxAll(gameObj.transform.position, boxSize, 0f);
        //    isBuildingOk = false;

        //    foreach (Collider2D collider in colliders)
        //    {
        //        if (!collider.isTrigger)
        //        {
        //            isBuildingOk = true;
        //            break;
        //        }
        //    }

        //    isBuildingOk = !isBuildingOk;

        //    if (isBuildingOk)
        //    {
        //        SetSlotColor(spriteRenderer, Color.green, 0.35f);
        //    }
        //    else
        //    {
        //        SetSlotColor(spriteRenderer, Color.red, 0.35f);
        //    }
        //}
    }

    protected virtual void InputCheck()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {//해당 좌표에 오브젝트가 있음 설치가 안되도록 수정해야함
            if (gameObj != null)
            {
                //if (isBuildingOk)
                {
                    GameObject obj = Instantiate(gameObj);
                    SetSlotColor(obj.GetComponentInChildren<SpriteRenderer>(), Color.white, 1f);
                    obj.transform.position = gameObj.transform.position;

                    if(obj.TryGetComponent(out Structure factory))
                    {
                        factory.SetBuild();
                        factory.EnableColliders();
                    }
                    else if (obj.TryGetComponent(out TowerAi tower))
                    {
                        tower.SetBuild();
                        tower.EnableColliders();
                    }
                    else if (obj.TryGetComponent(out BeltGroupMgr belt))
                    {
                        obj.transform.parent = beltMgr.transform;
                        belt.isPreBuilding = false;
                        belt.BeltList[0].SetBuild();
                        belt.BeltList[0].EnableColliders();
                    }
                    else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                    {
                        underBelt.beltScipt.SetBuild();
                        underBelt.beltScipt.EnableColliders();
                        underBelt.RemoveObj();
                    }
                    BuildingInfo.instance.BuildingEnd();    
                }
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

        if (gameObj.TryGetComponent(out Structure factory))
        {
            factory.isPreBuilding = true;
            factory.DisableColliders();
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
            belt.BeltList[0].DisableColliders();
        }
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.isPreBuilding = true;
            underBelt.SetSendUnderBelt();
            underBelt.beltScipt.DisableColliders();
        }

        Bounds objectBounds = gameObj.GetComponentInChildren<SpriteRenderer>().bounds;

        Vector2 size = objectBounds.size;
        boxSize = new Vector2(size.x, size.y);

        this.size = new Vector2Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));

        if (objectBounds.size.x % 2 == 0 || objectBounds.size.y % 2 == 0)
        {
            gameObj.transform.position = this.transform.position - setPos;
        }
        else
            gameObj.transform.position = this.transform.position;

        //만약 건물에 사이즈가 필요한경우 위에서 아래로 변경해야할수도
        //Vector3 bottomLeftTileCenter = new Vector3(
        //    -numColumns * 1 / 2f + 1 / 2f,
        //    -numRows * 1 / 2f + 1 / 2f,
        //    0f);

        //gameObj.transform.position = this.transform.position - bottomLeftTileCenter;

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
        //isBuildingOk = false;
        gameObj = null;
        gameObject.SetActive(false);
    }

    void RotationImg()
    {
        if (gameObj.TryGetComponent(out Structure factory))
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
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.beltScipt.dirNum++;
            if (underBelt.beltScipt.dirNum >= underBelt.beltScipt.dirCount)
                underBelt.beltScipt.dirNum = 0;

            underBelt.dirNum = underBelt.beltScipt.dirNum;
        }
    }

}
