using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour {

    private BoxCollider2D punchCollider;
    private PlayerMovement playerScript;

	// Use this for initialization
	void Start () {
        playerScript = transform.parent.GetComponent<PlayerMovement>();
        punchCollider = this.gameObject.GetComponent<BoxCollider2D>();
    }
	
	// Update is called once per frame
	void Update () {
        if (playerScript.attacking)
            punchCollider.enabled = true;
        else if (playerScript.attacking == false)
            punchCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Looks like there was a collision");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Thank god that's over");
    }
}
