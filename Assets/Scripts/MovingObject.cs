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
    public int rollLength = 41; // how many frames a roll lasts
    public int attackLength = 10;

    public LayerMask groundLayer; // The ground layers contains all terrain that the player might touch


    //private BoxCollider2D boxCollider; // Declare a variable for the box collider of the moving object
    private Rigidbody2D rb2d; // Declare a variable for the rigidbody of the moving object
    private BoxCollider2D boxCollider;
    private Transform groundCheck;
    private Animator animator;
    private bool grounded = false;
    private bool rolling = false;
    private bool running = false;
    private int rollTimer = 0; // timer to tell when the roll is over
    private bool sneaking = false; // whether the player is sneaking
    private bool attacking = false; // whether the player is attacking
    private int attackTimer = 0; // timer to tell when the attack is over
    private float moveHorizontal = 0f;

    // Use this for initialization
    protected virtual void Start() {
        //boxCollider = GetComponent<BoxCollider2D>(); // Get the box collider componenet
        rb2d = GetComponent<Rigidbody2D>(); // Get the rigidbody component
        boxCollider = GetComponent<BoxCollider2D>();
        groundCheck = transform.Find("groundCheck");
        animator = GetComponent<Animator>();

        // The bitshift gets the bitmask of the layer. This checks only the Ground layer
        // The opposite of this is ~groundLayer, which checks all layers except the Ground layer.
        // More info at https://docs.unity3d.com/Manual/Layers.html
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        //groundLayer = ~groundLayer;
    }

    void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = 0f;

        if (moveHorizontal != 0)
            running = true;
        else
            running = false;

        // Make the player character face the direction it's moving
        if (moveHorizontal > 0)
            transform.localScale = new Vector2(1, transform.localScale.y);
        else if (moveHorizontal < 0)
            transform.localScale = new Vector2(-1, transform.localScale.y);

        grounded = isGrounded();

        if (moveHorizontal != 0 && grounded && !sneaking)
            animator.SetBool("running", true);
        else
            animator.SetBool("running", false);

        if (Input.GetButtonDown("Jump") && grounded && !rolling)
        {
            moveVertical = jumpSpeed;
        }

        roll();
        attack();
        sneak();

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * acceleration); // add the current movement force to the player

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

        // limit speed if moving faster than max vertical speed
        if (Math.Abs(rb2d.velocity.y) > maxVerticalSpeed)
        {
            limitSpeed("y", rb2d, maxVerticalSpeed);
        }
  
    }

    void sneak()
    {
        if (Input.GetButtonDown("Sneak") && grounded && !rolling)
        {
            sneaking = true;
            transform.position = new Vector2(transform.position.x, transform.position.y - .5f);
        }

        if (sneaking)
        {
            animator.SetBool("crouching", true);
        }

        if (sneaking && (Input.GetButtonUp("Sneak") || !grounded))
        {
            sneaking = false;
            animator.SetBool("crouching", false);
        }

        if (!running && grounded && sneaking)
            animator.SetBool("sneaking", true);
        else if (running && grounded && sneaking)
            animator.SetBool("sneaking", false);
    }

    void attack()
    {
        // If the roll button is pressed and the character is grounded and the character isn't already rolling, then start the rolling timer and start rolling
        if ((Input.GetButtonDown("Attack") && grounded && !rolling && !sneaking && !attacking))
        {
            animator.SetBool("attacking1", true);
            attackTimer = attackLength;
            Debug.Log(attackTimer);
            attacking = true;
        }

        // If the character is attacking
        if (attacking)
        {
            // decrease attack timer by one
            attackTimer -= 1;

            // If the player is in the air, we want to stop attacking
            if (!grounded)
                attackTimer = 0;

            // if the roll timer is 0, then stop attacking
            if (attackTimer == 0)
            {
                attacking = false;
                animator.SetBool("attacking1", false);
            }

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
                rb2d.velocity = new Vector2(-maxSpeed, rb2d.velocity.y);
            else if (rb2d.velocity.y > 0f)
                rb2d.velocity = new Vector2(maxSpeed, rb2d.velocity.y);
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
        if ((Input.GetButtonDown("Roll") && grounded && !rolling && !sneaking && !attacking))
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
        if (!touchingWall())
            return;

        float moveVertical = Input.GetAxis("Vertical");
    }

    bool touchingWall()
    {
        Vector2 startPosition = rb2d.transform.position;
        Vector2 endPosition = new Vector2(rb2d.transform.position.x + 1, rb2d.transform.position.y);
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, groundLayer);

        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    bool isGrounded()
    {
        Vector2 startPosition = rb2d.transform.position;
        Vector2 endPosition = new Vector2(rb2d.transform.position.x, rb2d.transform.position.y - 1.35f);
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, groundLayer);

        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }
}
