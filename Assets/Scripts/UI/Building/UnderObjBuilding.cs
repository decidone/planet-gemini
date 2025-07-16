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

    bool setLine;
    public GameObject lineObj;
    public LineRenderer lineRenderer;
    protected Vector3 startLine;
    protected Vector3 endLine;
    PreBuilding preBuilding;

    void Start()
    {
        nonNetObj = GetComponent<PreBuildingImg>();
        preBuilding = PreBuilding.instance;
        isSendObj = true;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        CheckPos();
        if(isUnderBelt)
            CheckNearObj(checkPos[0]);
        else
            CheckNearObj(checkPos[2]);
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
                            EndRenderer();
                            return;
                        }
                    }
                    else if (factoryCollider.TryGetComponent(out SendUnderBeltCtrl othSendUnderBelt))
                    {
                        if (othSendUnderBelt.dirNum == dirNum)
                        {
                            IsGetObjSet();
                            othSendUnderBelt.EndRenderer();
                            if (!setLine)
                                StartRenderer(othSendUnderBelt.transform.position);
                            else
                                RendererReset(othSendUnderBelt.transform.position);
                            return;
                        }
                    }
                }
                else
                {
                    if (factoryCollider.TryGetComponent(out UnderPipeCtrl underPipe))
                    {
                        if (underPipe.dirNum == dirNum)
                        {
                            EndRenderer();
                            return;
                        }
                        else if ((underPipe.dirNum + 2) % 4 == dirNum) 
                        {
                            underPipe.EndRenderer(true);
                            if (!setLine)
                                StartRenderer(underPipe.transform.position);
                            else
                                RendererReset(underPipe.transform.position);
                            return;
                        }
                    }
                }
            }
        }

        if (isUnderBelt)
        {
            IsSendObjSet();
        }
        EndRenderer();
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

    void EndRenderer()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
        }
        setLine = false;
    }

    void StartRenderer(Vector3 target)
    {
        setLine = true;
        startLine = new Vector3(target.x, target.y, -1);
        GameObject currentLine = Instantiate(lineObj, startLine, Quaternion.identity);
        currentLine.transform.parent = transform;
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startLine);
        endLine = new Vector3(transform.position.x, transform.position.y, -1);
        lineRenderer.SetPosition(1, endLine);
    }

    void RendererReset(Vector3 target)
    {
        if (lineRenderer != null)
        {
            startLine = new Vector3(target.x, target.y, -1);
            lineRenderer.SetPosition(0, startLine);
            endLine = new Vector3(transform.position.x, transform.position.y, -1);
            lineRenderer.SetPosition(1, endLine);
        }
    }
}
