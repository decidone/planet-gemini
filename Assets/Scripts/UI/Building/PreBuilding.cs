using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PreBuilding : MonoBehaviour
{
    public static PreBuilding instance;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    GameObject gameObj = null; 
    public bool isSelect = false;

    public GameObject beltMgr = null;
    public GameObject pipeMgr = null;

    Vector3 setPos = new Vector3(-0.5f, -0.5f);

    Vector2 boxSize;
    public Vector2Int size; // 건물의 크기

    private Tilemap tilemap;

    void Awake()
    {
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();


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
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
        cellCenter.z = transform.position.z;
        transform.position = cellCenter;
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
                        belt.beltList[0].SetBuild();
                        belt.beltList[0].EnableColliders();
                    }
                    else if (obj.TryGetComponent(out PipeGroupMgr pipe))
                    {
                        obj.transform.parent = pipeMgr.transform;
                        pipe.isPreBuilding = false;
                        pipe.pipeList[0].SetBuild();
                        pipe.pipeList[0].EnableColliders();
                    }
                    else if (obj.TryGetComponent(out UnderBeltCtrl underBelt))
                    {
                        underBelt.beltScipt.SetBuild();
                        underBelt.EnableColliders();
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

    public void SetImage(GameObject game, int level)
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
            factory.level = level;
        }
        else if (gameObj.TryGetComponent(out TowerAi tower))
        {
            tower.isPreBuilding = true;
            tower.DisableColliders();
            tower.level = level;
        }
        else if (gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.isPreBuilding = true;
            belt.SetBelt(0);
            belt.beltList[0].isPreBuilding = true;
            belt.beltList[0].DisableColliders();
            belt.beltList[0].level = level;
        }
        else if (gameObj.TryGetComponent(out PipeGroupMgr pipe))
        {
            pipe.isPreBuilding = true;
            pipe.SetPipe(0);
            pipe.pipeList[0].isPreBuilding = true;
            pipe.pipeList[0].DisableColliders();
        }
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.isPreBuilding = true;
            underBelt.SetSendUnderBelt();
            //underBelt.SetLevel(level);
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
        if (gameObj.TryGetComponent(out Structure factory))
        {
            factory.dirNum++;
            if(factory.dirNum >= factory.dirCount)
                factory.dirNum = 0;
        }
        else if(gameObj.TryGetComponent(out BeltGroupMgr belt))
        {
            belt.beltList[0].dirNum++;
            if (belt.beltList[0].dirNum >= belt.beltList[0].dirCount)
                belt.beltList[0].dirNum = 0;
        }
        else if (gameObj.TryGetComponent(out UnderBeltCtrl underBelt))
        {
            underBelt.beltScipt.dirNum++;
            if (underBelt.beltScipt.dirNum >= underBelt.beltScipt.dirCount)
                underBelt.beltScipt.dirNum = 0;

            underBelt.dirNum = underBelt.beltScipt.dirNum;
        }
        else if (gameObj.TryGetComponent(out UnderPipeCtrl underPipe))
        {
            underPipe.dirNum++;
            if (underPipe.dirNum >= underPipe.dirCount)
                underPipe.dirNum = 0;
        }
    }

}
