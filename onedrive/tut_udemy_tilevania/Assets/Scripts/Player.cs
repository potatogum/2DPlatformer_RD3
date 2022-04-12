using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    // Config
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float jumpTime = 1f;
    [SerializeField] private float jumpStopForce = 2.5f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float gravity = 3.5f;
    [SerializeField] private Vector2 knockBackForce = new Vector2(10f, 15f);

    // State
    private bool isAlive = true;

    // Cache
    private Rigidbody2D rigidBody;
    private Animator animator;
    private PolygonCollider2D bodyCollider;
    //Trigger colliders
    [SerializeField] private Collider2D feetCollider;
    [SerializeField] private Collider2D headCollider;
    //[SerializeField] private Collider2D frontCollider;
    //[SerializeField] private Collider2D behindCollider;

    private float playerGravity;
    private float currentJumpTime;



    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<PolygonCollider2D>();
        playerGravity = rigidBody.gravityScale;
    }

    private void Update()
    {
        if (!isAlive) { return; }

        Jump();
    }
    private void FixedUpdate()
    {
        Run();
        Climb();
        FlipSprite();
    }

    /// <summary>
    /// Set the run speed according to the horizontal input axis
    /// </summary>
    private void Run()
    {
        float horizontalInput = CrossPlatformInputManager.GetAxis("Horizontal"); // value between -1 and 1
        rigidBody.velocity = new Vector2(horizontalInput*runSpeed, rigidBody.velocity.y);

        bool isMovingHorizontally = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
        animator.SetBool("Running", isMovingHorizontally);
    }

    private void Jump()
    {
        //if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))  return;

        if (CrossPlatformInputManager.GetButtonDown("Jump") && feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            rigidBody.velocity += new Vector2(0, jumpSpeed);
            currentJumpTime = jumpTime;
        }
        else if(rigidBody.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rigidBody.gravityScale = gravity * jumpStopForce;
        }
        else
        {
            rigidBody.gravityScale = gravity;
        }
    }

    private void Climb()
    {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Climbable")))
        {
            animator.SetBool("Climbing", false);
            animator.SetBool("OnLadder", false);
            return;
        }
        
        animator.SetBool("OnLadder", true);
        float verticalMoveDirection = Input.GetAxisRaw("Vertical");
        if (verticalMoveDirection != 0)
        {
            if (verticalMoveDirection < 0 && feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                return;
            else if (verticalMoveDirection > 0 && headCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                return;

            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            rigidBody.velocity = new Vector2(0, climbSpeed * verticalMoveDirection);//rigidBody.velocity.x
            animator.SetBool("Climbing", true);
        }
        else
        {
            rigidBody.bodyType = RigidbodyType2D.Kinematic;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
            animator.SetBool("Climbing", false);
        }
    }

    /// <summary> 
    /// Flips sprite if player moves horizontally
    /// </summary> 
    private void FlipSprite()
    {
        bool isMovingHorizontally = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
        if (isMovingHorizontally)
        {
            transform.localScale = new Vector2(Mathf.Sign(rigidBody.velocity.x), 1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        if (!isAlive) { return; }

        Collider2D activePlayerCollider = collider.otherCollider;

        if (activePlayerCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")))
        {
            KnockBack(collider.transform.localPosition);
            Die();
            
        }

        if (activePlayerCollider.IsTouchingLayers(LayerMask.GetMask("Hazard")))
        {
            Die();
        }
    }

    private void KnockBack(Vector3 objectPosition)
    {
        float knockDirection = transform.position.x - objectPosition.x < 0 ? -1 : 1;
        Vector2 forceWithDirection = new Vector2(knockBackForce.x * knockDirection, knockBackForce.y);
        rigidBody.AddForce(forceWithDirection, ForceMode2D.Impulse);
    }

    private void Die()
    {
        isAlive = false;
        animator.SetTrigger("Dead");
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }
}
