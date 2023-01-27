using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltItemCtrl : MonoBehaviour
{
    public bool isStop = false;
    //public List<Vector3> nextMove = new List<Vector3>();

    //float beltSpeed;

    //public bool isturn = false;

    //public BeltCtrl OnBelt = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    //private void FixedUpdate()
    //{
        //if(isStop == false)
        //    MoveItem();
    //}

    //void MoveItem()
    //{
    //    if(nextMove.Count > 0)
    //    {
    //        if (nextMove[0] != this.gameObject.transform.position)
    //            transform.position = Vector3.MoveTowards(this.gameObject.transform.position, nextMove[0], Time.deltaTime * beltSpeed);
    //        else if (nextMove[0] == this.gameObject.transform.position)
    //        {
    //            nextMove.RemoveAt(0);
    //            OnBelt.SetNextPos(GetComponent<BeltItemCtrl>());
    //        }
    //    }       
    ////}
    //public void GetPos(Vector2 nextPos, BeltCtrl belt)
    //{
    //    nextMove.Add(nextPos);
    //    OnBelt = belt;
    //    beltSpeed = belt.beltSpeed;
    //}
}
