using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tets : MonoBehaviour
{
    Vector3 start = new Vector3(0, 0, 0);
    Vector3 end = new Vector3(1, 0, 0);


    // Start is called before the first frame update
    void Start()
    {
        transform.position = start;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position != end)
            test();
    }

    void test()
    {
        transform.position = Vector3.MoveTowards(start, end, Time.deltaTime * 3);

    }

}
