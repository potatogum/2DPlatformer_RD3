using System.Collections;
using UnityEngine;


public class Player : MonoBehaviour
{
    // Config
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float jumpTime = 1f;
    [SerializeField] private float jumpStopForce = 2.5f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float inAirTurnTime = 2.1f;
    [SerializeField] private float climbSpeed = 3f;
 
    [SerializeField] private Vector2 knockBackForce = new Vector2(10f, 15f);


    // State
    private bool isAlive = true;

    // Cache
    private Rigidbody2D rigidBody;
    private Animator animator;
    private BoxCollider2D bodyCollider;

    [SerializeField] private Transform groundCheck;

    private float currentJumpTime;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isGrounded = false;
    private bool isBufferJumping;
    private bool useSlowTurnInAir;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isAlive) { return; }

        Jump();
    }
    private void FixedUpdate()
    {
        HorizontalMovement();
        //Climb();
        FlipSprite();
        
    }

    /// <summary>
    /// Set the run speed and horizontal jump speed according to the horizontal input axis
    /// </summary>
    private void HorizontalMovement()
    {
        //TODO: Use only GetAxis when in air and changing direction!
        //float horizontalInput = !useSlowTurnInAir ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            rigidBody.velocity = new Vector2(horizontalInput * runSpeed, rigidBody.velocity.y);
        }
        else
        {
            float xSpeed = Mathf.MoveTowards(rigidBody.velocity.x, horizontalInput * runSpeed, inAirTurnTime);
            rigidBody.velocity = new Vector2(xSpeed, rigidBody.velocity.y);
        }
        

        bool isMovingHorizontally = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
        animator.SetBool("Running", isMovingHorizontally);
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            DisableSlowTurnInAir();
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) 
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isBufferJumping)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed);

            jumpBufferCounter = 0f;

            StartCoroutine(BufferJumpCooldown());
        }

        if (Input.GetButtonUp("Jump") && rigidBody.velocity.y > 0f)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
        /*
        if (input.GatherInput().JumpDown && isGrounded)
        {
            rigidBody.gravityScale = 0;
            coyoteCounter = 0;
            rigidBody.velocity += new Vector2(0, jumpSpeed);
 
            currentJumpTime = jumpTime;
        }
        else if(rigidBody.velocity.y > 0 && !input.GatherInput().JumpHeld)
        {
            rigidBody.gravityScale = gravity * jumpStopForce;
        }
        else
        {
            rigidBody.gravityScale = gravity;
        }
        */
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, LayerMask.GetMask("Ground"));
    }

    private IEnumerator BufferJumpCooldown()
    {
        isBufferJumping = true;
        yield return new WaitForSeconds(0.4f);
        isBufferJumping = false;
    }


    /*private void Climb()
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
    }*/

    /// <summary> 
    /// Flips sprite if player moves horizontally
    /// </summary> 
    private void FlipSprite()
    {
        if (rigidBody.velocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (rigidBody.velocity.x > 0)
        {
            spriteRenderer.flipX = false;
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

    private void 
}
