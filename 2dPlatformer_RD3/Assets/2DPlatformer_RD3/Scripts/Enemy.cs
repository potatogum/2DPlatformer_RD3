using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    private Rigidbody2D rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Detection based on the BoxCollider2D set to trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        FlipSprite();
    }

    private void FlipSprite()
    {
        transform.localScale = new Vector2(-Mathf.Sign(transform.localScale.x), 1);
    }

    void FixedUpdate()
    {
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        bool isMovingRight = transform.localScale.x > 0;

        rigidBody.velocity = isMovingRight switch
        {    
            true => new Vector2(moveSpeed, 0), 
            false => new Vector2(-moveSpeed, 0)
        };
    }


}
