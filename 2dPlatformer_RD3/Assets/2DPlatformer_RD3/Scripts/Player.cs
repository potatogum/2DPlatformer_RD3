using System.Collections;
using UnityEngine;


public class Player : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private bool useLerpRunSpeed = false;
    [SerializeField] private float lerpRunSpeed = 1.8f;

    [Header("Jump")]
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float jumpTime = 1f;
    [SerializeField] private float jumpStopForce = 2.5f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float inAirTurnTime = 2.1f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isBufferJumping;


    [Header("Gravity")]
    [SerializeField] private float gravityScale = 6.2f;
    [SerializeField] private float fallMultiplier = 1.5f;
    [SerializeField] private float fallMultiplierLerpSpeed = 2f;

    [Header("Collision")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("The maybes")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private Vector2 knockBackForce = new Vector2(10f, 15f);


    // GameObject related
    private float absoluteScaleX;

    // State
    private bool isAlive = true;

    // Components
    private Rigidbody2D rigidBody;
    private Animator animator;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer spriteRenderer;

    // Animation states
    const string PLAYER_IDLE = "player_idle";
    const string PLAYER_RUN = "player_run";
    const string PLAYER_JUMP = "player_jump";
    const string PLAYER_WALL_IDLE = "player_wall_idle";
    const string PLAYER_WALL_CLIMB = "player_wall_climb";
    private string currentState;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        absoluteScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Update()
    {
        if (!isAlive) { return; }

        Jump();
    }
    private void FixedUpdate()
    {
        HorizontalMovement();
        AddFallMultiplier();
        //Climb();
        FlipSprite();
        HandleAnimator();

        if (IsTouchingWall()) Debug.Log("Touching");
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
            if (useLerpRunSpeed)
            {
                float xSpeed = Mathf.MoveTowards(rigidBody.velocity.x, horizontalInput * runSpeed, lerpRunSpeed);
                rigidBody.velocity = new Vector2(xSpeed, rigidBody.velocity.y);
            }
            else
            {
                rigidBody.velocity = new Vector2(horizontalInput * runSpeed, rigidBody.velocity.y);
            }
        }
        else
        {
            float xSpeed = Mathf.MoveTowards(rigidBody.velocity.x, horizontalInput * runSpeed, inAirTurnTime);
            rigidBody.velocity = new Vector2(xSpeed, rigidBody.velocity.y);
        }
        
        bool isMovingHorizontally = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
    }

    private void HandleAnimator()
    {
        if (IsGrounded())
        {
            if (rigidBody.velocity.x == 0) ChangeAnimationState(PLAYER_IDLE);
            else if (rigidBody.velocity.x != 0) ChangeAnimationState(PLAYER_RUN);
        }
        else
        {
            ChangeAnimationState(PLAYER_JUMP);
        }
    }
    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return; // prevent animation from interupting itself
        animator.Play(newState);
        currentState = newState;
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
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
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, LayerMask.GetMask("Ground"));
    }
    private bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, LayerMask.GetMask("Ground"));
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
        {   //spriteRenderer.flipX = true;
            transform.localScale = new Vector3(-absoluteScaleX, transform.localScale.y, 1);
        }
        else if (rigidBody.velocity.x > 0)
        {   //spriteRenderer.flipX = false;
            transform.localScale = new Vector3(absoluteScaleX, transform.localScale.y, 1); 
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

    private void AddFallMultiplier()
    {
        if (rigidBody.velocity.y < 0)
        {
            rigidBody.gravityScale = Mathf.MoveTowards(gravityScale, gravityScale * fallMultiplier, fallMultiplierLerpSpeed);
        }
        else
        {
            rigidBody.gravityScale = gravityScale;
        }
    }
}
