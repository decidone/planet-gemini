using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitterCtrl : MonoBehaviour
{
    public int dirNum = 0;    // πÊ«‚
    public Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    public BeltCtrl inBelt = null;
    public List<BeltCtrl> outBelt = new List<BeltCtrl>();

    public List<ItemProps> itemList = new List<ItemProps>();
    int fullItemNum = 10;
    public bool isFull = false;
    int getBeltNum = 0;

    bool itemGetDelay = false;
    bool itemSetDelay = false;
    float delaySpeed = 0.4f;

    // Start is called before the first frame update
    void Start()
    {
        setModel = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        SetDirNum();
        if (inBelt != null && isFull == false) 
        {
            if (itemGetDelay == false)
                StartCoroutine("GetBeltItem");
        }
        if (itemList.Count > 0 && outBelt.Count > 0)
        {
            if (itemSetDelay == false)
                StartCoroutine("SetBeltItem");
        }
    }

    void SetDirNum()
    {
        if (dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];
        }
    }

    public void SetBelt(BeltCtrl belt)
    {
        if (inBelt == null)
        {
            CheckInBelt();
            if (inBelt == null)
                outBelt.Add(belt);
        }
        else if (inBelt != null)
            outBelt.Add(belt);
    }

    void CheckInBelt()
    {
        var Check = transform.up;

        if (dirNum == 0)
        {
            Check = -transform.up;
        }
        else if (dirNum == 1)
        {
            Check = -transform.right;
        }
        else if (dirNum == 2)
        {
            Check = transform.up;
        }
        else if (dirNum == 3)
        {
            Check = transform.right;
        }

        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Check, 1f);

        if (hit)
        {
            if (hit.collider.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl belt = hit.collider.GetComponent<BeltCtrl>();
                if (belt.dirNum == dirNum)
                    inBelt = belt;
            }
        }
    }

    void AddItem(ItemProps item)
    {
        itemList.Add(item);
        item.transform.position = this.transform.position;

        if (itemList.Count >= fullItemNum)
            isFull = true;
    }

    IEnumerator GetBeltItem()
    {
        itemGetDelay = true;

        if (inBelt.isItemStop == true)
        {
            AddItem(inBelt.itemList[0]);
            inBelt.isItemStop = false;
            inBelt.itemList.RemoveAt(0);

            yield return new WaitForSeconds(delaySpeed);
            itemGetDelay = false;
        }
        else if (inBelt.isItemStop == false)
        {
            itemGetDelay = false;
            yield break;
        }

    }

    IEnumerator SetBeltItem()
    {
        itemSetDelay = true;

        if (outBelt[getBeltNum].isFull == false)
        {
            outBelt[getBeltNum].AddItem(itemList[0]);
            itemList.RemoveAt(0);

            getBeltNum++;
            if (getBeltNum >= outBelt.Count)
                getBeltNum = 0;        
            
            yield return new WaitForSeconds(delaySpeed);
            itemSetDelay = false;
        }
        else if(outBelt[getBeltNum].isFull == true)
        {
            getBeltNum++;
            if (getBeltNum >= outBelt.Count)
                getBeltNum = 0;

            itemSetDelay = false;
            yield break;
        }

        if (itemList.Count < fullItemNum)
            isFull = false;
    }
}
