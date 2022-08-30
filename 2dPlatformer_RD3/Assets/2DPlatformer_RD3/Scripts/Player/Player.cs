using System.Collections;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerAnimationController))]
    public class Player : MonoBehaviour
    {
        [Header("Horizontal Movement")]
        [SerializeField] private float runSpeed = 12f;
        [SerializeField] private bool useLerpRunSpeed = false;
        [SerializeField] private float lerpRunSpeed = 1.8f;

        [Header("Jump")]
        [SerializeField] private float jumpSpeed = 19f;
        [SerializeField] private float jumpTime = 1f;
        [SerializeField] private float jumpStopForce = 2.5f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;
        [SerializeField] private float inAirTurnTime = 2.1f;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool isJumping;

        [Header("WallJump")]
        [SerializeField] private Vector2 wallJumpSpeed;
        [SerializeField] private float wallJumpLockedBuffer = 0.24f;
        private float wallJumpLockedTime;
        private bool canMove = true;

        [Header("Dash")]
        [SerializeField] private float dashingPower = 24f;
        [SerializeField] private float dashingTime = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float straightUpDashPower = 0.9f;
        private bool canDash = true;
        private bool isDashing;

        [Header("Gravity")]
        [SerializeField] private float gravityScale = 6.2f;
        [SerializeField] private float fallMultiplier = 1.5f;
        [SerializeField] private float fallMultiplierLerpSpeed = 2f;

        [Header("Wall Climb")]
        [SerializeField] private float climbSpeed = 12f;
        private bool isWallClimbing = false;
        [SerializeField] private float cannotGrapOrSlideBuffer = 0.3f;
        private float cannotGrabOrSlideTime;
        private bool canGrabOrSlide = true;

        [Header("Wall Slide")]
        [SerializeField] private float slideSpeed = -3f;
        private bool isWallSliding = false;


        [Header("Collision")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform wallCheck;


        [Header("The maybes")]
        //[SerializeField] private float climbSpeed = 3f;
        [SerializeField] private Vector2 knockBackForce = new Vector2(10f, 15f);


        // GameObject related
        private float absoluteScaleX;

        // State
        private bool isAlive = true;
        private bool isGrounded;

        // Components
        private Rigidbody2D rigidBody;
        private BoxCollider2D bodyCollider;
        private SpriteRenderer spriteRenderer;
        private PlayerAnimationController playerAnim;


        private void Start()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerAnim = GetComponent<PlayerAnimationController>();
            absoluteScaleX = Mathf.Abs(transform.localScale.x);
        }

        private void Update()
        {
            if (!isAlive || isDashing) { return; }

            CanGrabOrSlideTimer();
            WallSlide();
            WallClimb();
            Jump();
            CheckDash();
        }
        private void FixedUpdate()
        {
            HandleAnimator();

            if (!isAlive || isDashing) 
                return;

            HorizontalMovement();
            AddFallMultiplier();
            //Climb();
            FlipSprite();

        }

        /// <summary>
        /// Set the run speed and horizontal jump speed according to the horizontal input axis
        /// </summary>
        private void HorizontalMovement()
        {
            if (!canMove)
                return;

            //TODO: Use only GetAxis when in air and changing direction!
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
            else if (!isWallClimbing)
            {
                float xSpeed = Mathf.MoveTowards(rigidBody.velocity.x, horizontalInput * runSpeed, inAirTurnTime);
                rigidBody.velocity = new Vector2(xSpeed, rigidBody.velocity.y);
            }

            bool isMovingHorizontally = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;
        }

        private void HandleAnimator()
        {
            string animationToPlay = "";
            // if (!isAlive) playerAnim.ChangeAnimationState(playerAnim.PLAYER_DEAD); // not created yet
            if (IsGrounded())
            {
                Debug.Log(rigidBody.velocity.x);
                //if (rigidBody.velocity.x == 0) animationToPlay = playerAnim.PLAYER_IDLE;
                if (Mathf.Abs(rigidBody.velocity.x) < 0.01) animationToPlay = playerAnim.PLAYER_IDLE;
                else if (rigidBody.velocity.x != 0) animationToPlay = playerAnim.PLAYER_RUN;
            }
            else
            {
                if (isWallSliding) animationToPlay = playerAnim.PLAYER_WALL_GRAB;
                if (isWallClimbing) animationToPlay = playerAnim.PLAYER_WALL_CLIMB;
                else animationToPlay = playerAnim.PLAYER_JUMP;
            }
            playerAnim.ChangeAnimationState(animationToPlay);
        }

        // Based on: https://www.youtube.com/watch?v=RFix_Kg2Di0&list=PLcxXHZgunPikkxiHH_V50tVNVwaewrwNJ&index=4 for coyote time and jump buffer
        private void Jump()
        {
            bool jumpBtnDown = Input.GetButtonDown("Jump");
            float inputHorizontal = Input.GetAxisRaw("Horizontal");

            // Coyote Time is a bit of extra time to jump when leaving the ground
            coyoteTimeCounter = IsGrounded() ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
            // Jump buffer enables you to press jump a bit before you land, then when you land the jump is executed
            jumpBufferCounter = jumpBtnDown ? jumpBufferTime : jumpBufferCounter -= Time.deltaTime;


            /** Initiate a normal jump */
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed);
                jumpBufferCounter = 0f;
                StartCoroutine(JumpCooldown()); // where isJumping is set
            }
            /** Shorten the jump */
            if (Input.GetButtonUp("Jump") && rigidBody.velocity.y > 0f)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }


            // Prevent WallGrab and WallSlide when jumping while grounded and touching the wall
            if (IsPushing() && jumpBtnDown)
                StartCannotGrabOrSlideTimer();


            /** Wall slide jump */
            if (jumpBtnDown && isWallSliding)
            {
                isWallSliding = false;
                wallJumpLockedTime = wallJumpLockedBuffer;
                rigidBody.velocity = new Vector2(runSpeed * -inputHorizontal, jumpSpeed);
            }
            if (wallJumpLockedTime > 0)
            {
                wallJumpLockedTime -= Time.deltaTime;
                canMove = false;
            }
            else
            {
                canMove = true;
            }
            // END Wall slide jump

            /** Wall climb jump */
            if (jumpBtnDown && isWallClimbing)
            {
                isWallClimbing = false;
                StartCannotGrabOrSlideTimer();

                rigidBody.velocity = new Vector2(runSpeed * inputHorizontal, jumpSpeed);
            }
            // END Wall climb jump
        } 

        private void CheckDash()
        {
            if (IsGrounded()) 
                canDash = true;

            if (Input.GetButtonDown("Dash") && canDash)
                StartCoroutine(Dash());
        }


        private IEnumerator Dash()
        {
            // Set dash direction based on input or localScale if no input. (instead of localScale, set it according to is IsTurningRight())
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input.Normalize();
            Vector2 dashDirection = input != Vector2.zero ? input : new Vector2(transform.localScale.x, 0f);
            if (dashDirection == Vector2.up)
                dashDirection = new Vector2(0f, straightUpDashPower);

            // perform the dash (based on https://www.youtube.com/watch?v=2kFGmuPHiA0 (bendux))
            canDash = false;
            isDashing = true;
            rigidBody.gravityScale = 0f;
            rigidBody.velocity = dashDirection * dashingPower;
            // trailRenderer.emitting = true;xc
            yield return new WaitForSeconds(dashingTime);
            rigidBody.velocity *= 0.5f; // added this line to make it less floaty at the end
            // trailRenderer.emitting = false;
            rigidBody.gravityScale = gravityScale;
            isDashing = false;
        }

        private bool IsTurningRight()
        {
            return transform.localScale.x > 0;
        }

        private bool IsPushing()
        {
            return (IsGrounded() && IsTouchingWall());
        }

        private void StartCannotGrabOrSlideTimer()
        {
            cannotGrabOrSlideTime = cannotGrapOrSlideBuffer;
        }
        /** A simple timer to disable wall slide and grabbing when the player is grounded and touching a wall,
            enabling a normal jump straight up. */
        private void CanGrabOrSlideTimer()
        {
            if (cannotGrabOrSlideTime > 0)
            {
                canGrabOrSlide = false;
                cannotGrabOrSlideTime -= Time.deltaTime;
            }
            else
            {
                canGrabOrSlide = true;
            }
        }

        private void WallSlide()
        {
            if (!canMove || isWallClimbing || !canGrabOrSlide)
                return;

            isWallSliding = false;
            if (IsTouchingWall() && Input.GetAxisRaw("Horizontal") != 0 && !IsGrounded())
            {
                isWallSliding = true;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, slideSpeed);
            }
        }

        private void WallClimb()
        {
            if (!canGrabOrSlide)
                return;

            // Checking if we are climbing or not - if we are, gravity is set to 0, etc.  
            if (IsTouchingWall() && Input.GetButton("Grab") && !IsPushing()) // (IsPushing() is here to prevent jump from being performed without gravity when holding "grab" key)
            {
                isWallClimbing = true;
                rigidBody.gravityScale = 0;
                rigidBody.velocity = Vector2.zero;
            }
            else
            {
                isWallClimbing = false;
                rigidBody.gravityScale = gravityScale; //This is set in WallSlide
            }

            // The climb movement, up and down.
            if (isWallClimbing)
            {
                float velocityY = Input.GetAxisRaw("Vertical") * climbSpeed;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, velocityY);
            }
        }

        private bool IsGrounded()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, LayerMask.GetMask("Ground"));
            return Physics2D.OverlapCircle(groundCheck.position, 0.2f, LayerMask.GetMask("Ground"));
        }
        private bool IsTouchingWall()
        {
            return Physics2D.OverlapCircle(wallCheck.position, 0.2f, LayerMask.GetMask("Ground"));
        }

        /** Prevents spamming the space key from making the character jump higher. */
        private IEnumerator JumpCooldown()
        {
            isJumping = true;
            yield return new WaitForSeconds(0.4f);
            isJumping = false;
        }


        /** Flips sprite if player moves horizontally */
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
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }

        private void AddFallMultiplier()
        {
            if (isWallClimbing) return;

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
}
