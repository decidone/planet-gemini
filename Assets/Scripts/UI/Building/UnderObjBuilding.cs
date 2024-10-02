using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderObjBuilding : MonoBehaviour
{
    public bool isSendObj;
    Vector2[] checkPos = new Vector2[4];
    public int dirNum;
    int tempDir;
    bool isUnderBelt;

    PreBuildingImg nonNetObj;
    List<Sprite> spriteList = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        nonNetObj = GetComponent<PreBuildingImg>();
        isSendObj = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        CheckPos();
        CheckNearObj(checkPos[0]);
    }

    void CheckPos()
    {
        Vector2[] dirs = { Vector2.down, Vector2.left, Vector2.up, Vector2.right };

        for (int i = 0; i < 4; i++)
        {
            checkPos[i] = dirs[(dirNum + i) % 4];
        }
    }

    public void Setting(int _dirNum, bool _isUnderBelt, List<Sprite> sprites)
    {
        dirNum = _dirNum;
        tempDir = dirNum;
        isUnderBelt = _isUnderBelt;
        spriteList = sprites;
    }

    void CheckNearObj(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, 10);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D factoryCollider = hits[i].collider;

            if (factoryCollider.CompareTag("Factory") && factoryCollider.gameObject != gameObject
                && factoryCollider.gameObject.transform.position != gameObject.transform.position)
            {
                if (isUnderBelt)
                {
                    if (factoryCollider.TryGetComponent(out GetUnderBeltCtrl othGetUnderBelt))
                    {
                        if(othGetUnderBelt.dirNum == dirNum)
                        {
                            IsSendObjSet();
                            return;
                        }
                    }
                    else if (factoryCollider.TryGetComponent(out SendUnderBeltCtrl othSendUnderBelt))
                    {
                        if (othSendUnderBelt.dirNum == dirNum)
                        {
                            IsGetObjSet();
                            return;
                        }
                    }
                }
            }
            else if (factoryCollider.CompareTag("PreBuildingImg") && factoryCollider.gameObject != gameObject
                && factoryCollider.gameObject.transform.position != gameObject.transform.position)
            {
                if (isUnderBelt)
                {
                    if(factoryCollider.TryGetComponent(out UnderObjBuilding othUnderBelt)) 
                    {
                        if (othUnderBelt.isSendObj)
                        {
                            IsGetObjSet();
                            return;
                        }
                        else
                        {
                            IsSendObjSet();
                            return;
                        }
                    }
                }
                else
                {
                    if (factoryCollider.TryGetComponent(out UnderObjBuilding othUnderBelt))
                    {
                        TurnDir(othUnderBelt.dirNum);
                        return;
                    }
                }
            }
        }

        if(isUnderBelt)
            IsSendObjSet();
    }

    void IsSendObjSet()
    {
        nonNetObj.PreSpriteSet(spriteList[dirNum]);
        isSendObj = true;
    }

    void IsGetObjSet()
    {
        nonNetObj.PreSpriteSet(spriteList[dirNum + 4]);
        isSendObj = false;
    }

    void TurnDir(int preDir)
    {
        if (dirNum == 0 || dirNum == 2)
        {
            if (preDir == 0)
            {
                dirNum = 2;
            }
            else if (preDir == 2)
            {
                dirNum = 0;
            }
        }
        else if (dirNum == 1 || dirNum == 3)
        {
            if (preDir == 1)
            {
                dirNum = 3;
            }
            else if (preDir == 3)
            {
                dirNum = 1;
            }
        }

        if(tempDir == dirNum)        
            isSendObj = true;
        else
            isSendObj = false;

        nonNetObj.PreSpriteSet(spriteList[dirNum]);
    }
}
