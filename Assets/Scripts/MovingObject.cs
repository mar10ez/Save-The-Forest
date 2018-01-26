using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

	public float acceleration = 5f;
	public float maxSpeed = 20f; // Max speed of the character
	public float speed = 0f;
	public float jumpSpeed = 5f;
	public LayerMask blockingLayer; // The blocking layer contains all objects that might block a moving unit

	//private BoxCollider2D boxCollider; // Declare a variable for the box collider of the moving object
	private Rigidbody2D rb2d; // Declare a variable for the rigidbody of the moving object

	// Use this for initialization
	protected virtual void Start () {
		//boxCollider = GetComponent<BoxCollider2D>(); // Get the box collider componenet
		rb2d = GetComponent<Rigidbody2D>(); // Get the rigidbody component
	}

	void FixedUpdate() {
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = 0f;

		if (Input.GetButtonDown ("Jump") && rb2d.transform.position.y < 1f) {
			Debug.Log ("Jump button pressed");
			moveVertical = jumpSpeed;
		}

		Vector2 movement = new Vector2 (moveHorizontal, moveVertical);

		rb2d.AddForce (movement * acceleration); // add the current movement force to the player
		if(Math.Abs(rb2d.velocity.x) > maxSpeed)
		{
			
			if (rb2d.velocity.x < 0f)
				rb2d.velocity = new Vector2 (-maxSpeed, rb2d.velocity.y);
			else if (rb2d.velocity.x > 0f) 
				rb2d.velocity = new Vector2 (maxSpeed, rb2d.velocity.y);
			
			//rb2d.velocity = rb2d.velocity.normalized * maxSpeed; // If the player has passed the max speed, decrease to max speed
		}
	}
	/**
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position; // The start is the current position of the object
		Vector2 end = start + new Vector2(xDir, yDir); // The end is the current position, plus the move they make

		boxCollider.enabled = false; // disable the object's collider so it doesn't detect itself as a collision
		hit = Physics2D.Linecast(start, end, blockingLayer); // check for any collisions between the start and the end point, but only in the blocking layer because that's where collision objects are.
		boxCollider.enabled = true; // re-enable the object's collider

		if (hit.transform == null) // If there were no collisions, then the object is able to move
		{
			StartCoroutine(SmoothMovement(end));
			return true;
		}

		return false; // If there were collisions, then the object can't move

	}
	**/

	//protected abstract void OnCantMove<T>(T component) // A function that tells the object what to do if it can't move. This will be unique depending on the unit type, so it needs to be abstract.
	//	where T : Component;
}
