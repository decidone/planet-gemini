using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergerCtrl : MonoBehaviour
{
    public int dirNum = 0;    // πÊ«‚
    public Sprite[] modelNum = new Sprite[4];
    SpriteRenderer setModel;

    public List<BeltCtrl> inBelt = new List<BeltCtrl>();
    public BeltCtrl outBelt = null;

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
        if (inBelt.Count > 0 && isFull == false)
        {
            if (itemGetDelay == false)
                StartCoroutine("GetBeltItem");
        }
        if(itemList.Count > 0 && outBelt != null && outBelt.isFull == false)
        {
            if(itemSetDelay == false)            
                StartCoroutine("SetBeltItem");            
        }
    }

    void SetDirNum()
    {
        if(dirNum < 4)
        {
            setModel.sprite = modelNum[dirNum];
        }
    }

    public void SetBelt(BeltCtrl belt)
    {
        if(outBelt == null)
        {
            CheckOutBelt();
            if (outBelt == null)
                inBelt.Add(belt);
        }
        else if(outBelt != null)
            inBelt.Add(belt);
    }

    void CheckOutBelt()
    {
        var Check = transform.up;

        if (dirNum == 0)
        {
            Check = transform.up;
        }
        else if (dirNum == 1)
        {
            Check = transform.right;
        }
        else if (dirNum == 2)
        {
            Check = -transform.up;
        }
        else if (dirNum == 3)
        {
            Check = -transform.right;
        }

        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Check, 1f);

        if (hit)
        {
            if (hit.collider.GetComponent<BeltCtrl>() != null)
            {
                BeltCtrl belt = hit.collider.GetComponent<BeltCtrl>();
                if(belt.dirNum == dirNum)
                    outBelt = belt;
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

        if (inBelt[getBeltNum].isItemStop == true)
        {
            AddItem(inBelt[getBeltNum].itemList[0]);
            inBelt[getBeltNum].isItemStop = false;
            inBelt[getBeltNum].itemList.RemoveAt(0);       
            
            getBeltNum++;
            if (getBeltNum >= inBelt.Count)
                getBeltNum = 0;

            yield return new WaitForSeconds(delaySpeed);
            itemGetDelay = false;
        }
        else if (inBelt[getBeltNum].isItemStop == false)
        {
            getBeltNum++;
            if (getBeltNum >= inBelt.Count)
                getBeltNum = 0;          
            
            itemGetDelay = false;
            yield break;
        }
    }

    IEnumerator SetBeltItem()
    {
        itemSetDelay = true;

        outBelt.AddItem(itemList[0]);
        itemList.RemoveAt(0);

        if (itemList.Count < fullItemNum)
            isFull = false;

        yield return new WaitForSeconds(delaySpeed);
        itemSetDelay = false;
    }
}
