using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float acceleration = 5f; // how quickly the character accelerates
    public float maxMoveSpeed = 20f; // Max speed of the character
    public float maxCrouchSpeed = 10f;
    public float maxRollSpeed = 25f; // Max speed of the character while rolling
    public float maxVerticalSpeed = 10f; // how quickly the y coordinate of the character can change
    public float jumpSpeed = 5f; // How much force is applied when the character jumps
    public int rollLength = 5; // how many frames a roll lasts

    public LayerMask groundLayer; // The ground layers contains all terrain that the player might touch


    //private BoxCollider2D boxCollider; // Declare a variable for the box collider of the moving object
    private Rigidbody2D rb2d; // Declare a variable for the rigidbody of the moving object
    private BoxCollider2D boxCollider;
    private Transform groundCheck;
    private bool grounded = false;
    private bool rolling = false;
    private int rollTimer = 0;
    private bool crouching = false;

    // Use this for initialization
    protected virtual void Start() {
        //boxCollider = GetComponent<BoxCollider2D>(); // Get the box collider componenet
        rb2d = GetComponent<Rigidbody2D>(); // Get the rigidbody component
        boxCollider = GetComponent<BoxCollider2D>();
        groundCheck = transform.Find("groundCheck");

        // The bitshift gets the bitmask of the layer. This checks only the Ground layer
        // The opposite of this is ~groundLayer, which checks all layers except the Ground layer.
        // More info at https://docs.unity3d.com/Manual/Layers.html
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = 0f;

        grounded = isGrounded();
        //Debug.Log(grounded);

        if (Input.GetButtonDown("Jump") && grounded && !rolling)
        {
            moveVertical = jumpSpeed;
        }

        roll();

        if (Input.GetButtonDown("Crouch") && grounded && !rolling)
        {
            crouching = true;
            transform.position = new Vector2(transform.position.x, transform.position.y - .5f);
        }

        if (crouching)
        {
            transform.localScale = new Vector2(1f, .5f);
        }

        if(crouching && (Input.GetButtonUp("Crouch") || !grounded))
        {
            crouching = false;
            transform.localScale = new Vector2(1f, 1f);
        }

        if (false)
        {
            finalCollisionCheck();
            rb2d.velocity = new Vector2(0, moveVertical);
            return;
        }
        else
        {
            Vector2 movement = new Vector2(moveHorizontal, moveVertical);
            rb2d.AddForce(movement * acceleration); // add the current movement force to the player

            if (Math.Abs(rb2d.velocity.x) > maxCrouchSpeed && crouching)
            {
                limitSpeed("x", rb2d, maxCrouchSpeed);
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
        if ((Input.GetButtonDown("Roll") && grounded && !rolling && !crouching))
        {
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

            // 
            if (rollTimer == 0)
                rolling = false;
        }
    }

    private void finalCollisionCheck()
    {
        // Get the velocity
        Vector2 moveDirection = new Vector2(rb2d.velocity.x * Time.fixedDeltaTime + 1, 0.2f);

        // Get bounds of Collider
        var bottomRight = new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.max.y);
        var topLeft = new Vector2(boxCollider.bounds.min.x, boxCollider.bounds.min.y);

        // Move collider in direction that we are moving
        bottomRight += moveDirection;
        topLeft += moveDirection;

        // Check if the body's current velocity will result in a collision
        if (Physics2D.OverlapArea(topLeft, bottomRight, groundLayer))
        {
            Debug.Log(rb2d.velocity.y);
            // If so, stop the movement
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }
        else
            Debug.Log("Stop printing");
    }

    bool isGrounded()
    {
        Vector2 startPosition = rb2d.transform.position;
        Vector2 endPosition = new Vector2(rb2d.transform.position.x, rb2d.transform.position.y - 1.35f);
        Debug.Log(boxCollider.size.y / 2f);
        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, groundLayer);

        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }
}
