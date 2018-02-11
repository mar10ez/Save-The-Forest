using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyDamage : MonoBehaviour {

    public int hp = 5;

    public void damageEnemy(int damageTaken)
    {
        hp -= damageTaken;
        Debug.Log(damageTaken);
        Debug.Log(hp);
    }
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0) {
            Destroy(this.gameObject);
        }
	}
}
