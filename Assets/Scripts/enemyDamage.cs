using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyDamage : MonoBehaviour {

    public int hp = 5;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void damageEnemy(int damageTaken)
    {
        hp -= damageTaken;
    }
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0) {
            animator.SetTrigger("dying");
        }
	}
}
