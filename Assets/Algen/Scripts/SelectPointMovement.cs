using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPointMovement : MonoBehaviour
{
    float movementRange = 0.5f; // 움직일 범위
    float speed = 0.5f; // 움직이는 속도

    public Vector3 initialPosition;

    void Update()
    {
        // 오브젝트를 위아래로 움직임
        float newY = initialPosition.y + Mathf.PingPong(Time.time * speed, movementRange);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
