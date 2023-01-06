using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 8f;
    public Rigidbody2D rb;
    public Animator animator;

    public Vector2 movement;
    private float timer;
    
    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // idle 모션 방향을 위해 마지막 움직인 방향을 저장
        timer += Time.deltaTime;
        if (movement.x == 1|| movement.x == -1 || movement.y == 1 || movement.y == -1)
        {
            // 0.1초마다 입력 상태를 저장
            if(timer > 0.1)
            {
                animator.SetFloat("lastMoveX", movement.x);
                animator.SetFloat("lastMoveY", movement.y);
                timer = 0;
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
