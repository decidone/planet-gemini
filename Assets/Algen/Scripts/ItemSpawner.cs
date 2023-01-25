using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public List<Item> itemList = new List<Item>();
    public List<BeltCtrl> beltList = new List<BeltCtrl>();
    public GameObject itemPref;

    bool spawnDelay = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (beltList.Count > 0)
        {
            if(spawnDelay == false)
            {
                spawnDelay = true;
                StartCoroutine("ItemSpawn");
            }
        }
    }
    
    IEnumerator ItemSpawn()
    {
        if (beltList[0].isFull == false)
        {
            GameObject spawnItem = Instantiate(itemPref);
            SpriteRenderer sprite = spawnItem.GetComponent<SpriteRenderer>();
            sprite.sprite = itemList[0].icon;
            ItemProps itemProps = spawnItem.GetComponent<ItemProps>();
            itemProps.item = itemList[0];
            itemProps.amount = 1;
            spawnItem.transform.position = this.transform.position;
            spawnItem.AddComponent<BeltItemCtrl>();
            beltList[0].AddItem(spawnItem.GetComponent<BeltItemCtrl>());
            beltList[0].beltGroupMgr.GetComponent<BeltGroupMgr>().AddItem(spawnItem.GetComponent<BeltItemCtrl>());
        }

        yield return new WaitForSecondsRealtime(1.0f);
        spawnDelay = false;
    }

    public void AddBeltList(BeltCtrl beltCtrl)
    {
        beltList.Add(beltCtrl);
    }
}
