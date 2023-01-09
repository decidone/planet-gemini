using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltItemCtrl : MonoBehaviour
{

    bool isEnd = false;
    public bool Stop = false;
    public bool setItem;
    public List<Vector3> nextMove = new List<Vector3>();
    public BeltCtrl beltCtrl = null;
    //Vector2 keepPos;

    // Start is called before the first frame update
    void Start()
    {
        setItem = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if(isEnd == false && Stop == false)
            MoveItem();
    }

    void MoveItem()
    {
        if(nextMove.Count > 0)
        {
            if (nextMove[0] != this.gameObject.transform.position)
                transform.position = Vector3.MoveTowards(this.gameObject.transform.position, nextMove[0], Time.deltaTime);
            else if (nextMove[0] == this.gameObject.transform.position)
                nextMove.RemoveAt(0);
            if(nextMove.Count > 2)
                nextMove.RemoveAt(1);     
        }       
    }
    public void GetPos(Vector2 nextPos)
    {
        nextMove.Add(nextPos);
    }
}
