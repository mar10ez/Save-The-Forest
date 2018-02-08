using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float acceleration = 5f; // how quickly the character accelerates
    public float maxMoveSpeed = 20f; // Max speed of the character
    public float maxsneakSpeed = 10f;
    public float maxRollSpeed = 25f; // Max speed of the character while rolling
    public float maxVerticalSpeed = 10f; // how quickly the y coordinate of the character can change
    public float jumpSpeed = 5f; // How much force is applied when the character jumps
    public float maxClimbSpeed = 1f;
    public int rollLength = 41; // how many frames a roll lasts
    public int landingLag = 10; // After landing from a jump, how long does it take before the player can move again
    
    public LayerMask groundLayer; // The ground layers contains all terrain that the player might touch
    public LayerMask wallLayer;


    //private BoxCollider2D playerCollider; // Declare a variable for the box collider of the moving object
    private Rigidbody2D rb2d; // Declare a variable for the rigidbody of the moving object
    private BoxCollider2D playerCollider;
    private Animator animator;
    private bool grounded = false;
    private bool rolling = false;
    private bool running = false;
    private bool climbing = false;
    private int rollTimer = 0; // timer to tell when the roll is over
    private bool crouching = false;
    private bool sneaking = false; // whether the player is sneaking
    public bool attacking = false; // whether the player is attacking
    private int attackTimer = 0; // timer to tell when the attack is over

    private float moveHorizontal;
    private float moveVertical;

    // Use this for initialization
    protected virtual void Start() {
        //playerCollider = GetComponent<BoxCollider2D>(); // Get the box collider componenet
        rb2d = GetComponent<Rigidbody2D>(); // Get the rigidbody component
        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        // The bitshift gets the bitmask of the layer. This checks only the Ground layer
        // The opposite of this is ~groundLayer, which checks all layers except the Ground layer.
        // More info at https://docs.unity3d.com/Manual/Layers.html
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        
        //groundLayer = ~groundLayer;
        wallLayer = 1 << LayerMask.NameToLayer("Wall");

        moveHorizontal = 0f;
        moveVertical = 0f;
    }

    void FixedUpdate() {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = 0f;

        grounded = isGrounded(); // Check if the player is grounded, this is one of the first things that should happen in a frame
        run();
        jump();
        roll();
        attack();
        sneak();
        climbWall();

        if (canClimbWall())
            moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * acceleration); // add the current movement force to the player

        checkSpeedLimits();
    }

    void checkSpeedLimits()
    {
        if (Math.Abs(rb2d.velocity.x) > maxsneakSpeed && sneaking)
        {
            limitSpeed("x", rb2d, maxsneakSpeed);
        }
        // limit speed if moving faster than max speed and not rolling
        else if (Math.Abs(rb2d.velocity.x) > maxMoveSpeed && !rolling)
        {
            limitSpeed("x", rb2d, maxMoveSpeed);
        }
        // limit speed if rolling faster than max roll speed
        else if (Math.Abs(rb2d.velocity.x) > maxRollSpeed)
        {
            limitSpeed("x", rb2d, maxRollSpeed);
        }

        if (Math.Abs(rb2d.velocity.y) > maxClimbSpeed && canClimbWall())
        {
            limitSpeed("y", rb2d, maxClimbSpeed);
        }
        // limit speed if moving faster than max vertical speed
        else if (Math.Abs(rb2d.velocity.y) > maxVerticalSpeed)
        {
            limitSpeed("y", rb2d, maxVerticalSpeed);
        }
    }

    void run()
    {
        if (moveHorizontal != 0)
            running = true;
        if (moveHorizontal == 0 && grounded)
            rb2d.velocity = new Vector2(0f, rb2d.velocity.y);
            running = false;
        if (moveHorizontal == 0)
        

        // Make the player character face the direction it's moving
        if (moveHorizontal > 0)
            transform.localScale = new Vector2(1, transform.localScale.y);
        else if (moveHorizontal < 0)
            transform.localScale = new Vector2(-1, transform.localScale.y);

        if (moveHorizontal != 0 && grounded && !sneaking)
            animator.SetBool("running", true);
        else
            animator.SetBool("running", false);
    }

    void jump()
    {
        if (Input.GetButton("Jump") && grounded && !rolling)
        {
            animator.SetTrigger("jumping");
            moveVertical = jumpSpeed;
        }
    }

    void sneak()
    {
        if (Input.GetButton("Sneak") && grounded && !rolling)
        {
            crouching = true;
            transform.position = new Vector2(transform.position.x, transform.position.y - .5f);
            animator.SetBool("crouching", true);
        }
     
        if (crouching && moveHorizontal != 0)
        {
            animator.SetBool("sneaking", true);
        }
        
        if (crouching && (!Input.GetButton("Sneak") || !grounded))
        {
            sneaking = false;
            animator.SetBool("sneaking", false);
            crouching = false;
            animator.SetBool("crouching", false);
            
        }
    }

    void attack()
    {
        // If the roll button is pressed and the character is grounded and the character isn't already rolling, then start the rolling timer and start rolling
        if ((Input.GetButton("Attack") && grounded && !rolling && !sneaking))
        {
            animator.SetBool("attacking", true);
            attacking = true;
        }

        if ((!Input.GetButton("Attack") && attacking) || !grounded || rolling || sneaking)
        {
            animator.SetBool("attacking", false);
            attacking = false;
        }

    }

    // Function to limit the speed of the character if they move too fast in any direction.
    void limitSpeed(String dimension, Rigidbody2D rb2d, float maxSpeed)
    // The arguments are dimension (either "x" or "y"), the rigidbody of the character, and the maxSpeed. This max speed can be the max air speed, ground speed, or rolling speed
    {
        if (dimension == "x")
        {
            // If the velocity is negative, then set it to the negative limit of the velocity.
            if (rb2d.velocity.x < 0f)
            // Since it's the x velocity that's too high, we adjust it while leaving the y velocity alone.
                rb2d.velocity = new Vector2(-maxSpeed, rb2d.velocity.y);
            // otherwise it's positive.
            else if (rb2d.velocity.x > 0f)
                rb2d.velocity = new Vector2(maxSpeed, rb2d.velocity.y);
        }
        else if (dimension == "y")
        {
            if (rb2d.velocity.y < 0f)
                rb2d.velocity = new Vector2(rb2d.velocity.x, -maxSpeed);
            else if (rb2d.velocity.y > 0f)
                rb2d.velocity = new Vector2(rb2d.velocity.x, maxSpeed);
        }
        else
        {
            throw new Exception("The limit speed function was called with a dimension that isn't x or y");
        }
        return;
    }

    void roll()
    {
        // If the roll button is pressed and the character is grounded and the character isn't already rolling, then start the rolling timer and start rolling
        if ((Input.GetButton("Roll") && grounded && !rolling && !sneaking && !attacking))
        {
            animator.SetBool("rolling", true);
            rollTimer = rollLength;
            rolling = true;
        }

        // If the character is rolling
        if (rolling)
        {
            // decrease roll timer by one
            rollTimer -= 1;

            // If the player is in the air, we want to stop rolling
            if (!grounded)
                rollTimer = 0;

            // if the roll timer is 0, then stop rolling
            if (rollTimer == 0)
            {
                rolling = false;
                animator.SetBool("rolling", false);
            }
                
        }
    }

    void climbWall()
    {
        if (!canClimbWall())
        {
            animator.SetBool("climbing", false);
            return;
        }
            
        else
        {
            float moveVertical = Input.GetAxis("Vertical");
            animator.SetBool("climbing", true);
        }  
    }

    // Check if the player is touching a wall, will be used for wall climbing
    bool canClimbWall()
    {
        Vector2 startPosition = rb2d.transform.position;
        //startPosition = new Vector2(startPosition.x, startPosition.y + 1.35f);
        float rotation = transform.localScale.x;
        // If the player is facing right, check for walls to the right of the player

        Vector2 endPosition = new Vector2(rb2d.transform.position.x + (.625f * rotation), rb2d.transform.position.y - 1.35f);
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, wallLayer);

        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    // Check if the player is grounded. Used for jumping mechanics and the like
    bool isGrounded()
    {
        Vector2 startPosition = rb2d.transform.position; // get the current position of the player
        Vector2 endPosition = new Vector2(rb2d.transform.position.x, rb2d.transform.position.y - 1.35f); // get the position of the player's feet
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, groundLayer); 
        // draw a line directly beneath the player's feet. If it hits anything, we know the player is grounded

        // If the player is grounded, tell the animator, then return false
        if (hit.collider != null)
        {
            animator.SetBool("grounded", true);
            return true;
        }
        // If the player isn't grounded, tell the animator, then return false
        animator.SetBool("grounded", false);
        return false;
    }

}
