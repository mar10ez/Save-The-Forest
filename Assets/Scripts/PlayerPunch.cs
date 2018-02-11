using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour {

    private BoxCollider2D punchCollider;
    private PlayerMovement playerScript;

	// Use this for initialization
	void Start () {
        punchCollider = GetComponent<BoxCollider2D>();
        playerScript = transform.parent.GetComponent<PlayerMovement>();
    }
	
	// Update is called once per frame
	void Update () {
        if (playerScript.attacking)
            punchCollider.size = new Vector2(1.5f, .7f);
        else if (playerScript.attacking == false)
            punchCollider.size = new Vector2(0, 0);
    }

    private void OnTriggerEnter2D(Collider2D other) // What to do when the player punch hitbox collides with an object
    {
        if (other.tag == "Enemy")
        {
            // Grab the enemy game object
            GameObject enemy = other.gameObject;
            // enemy.GetComponent<enemyDamage>() grabs the script to damage the enemy
            // From there, I need to call the enemy damage function, and pass in the punch damage from the player script
            enemy.GetComponent<enemyDamage>().damageEnemy(playerScript.punchDamage);
        }

        else if (other.tag == "Destructible")
        {

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }
}
