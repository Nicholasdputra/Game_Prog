using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{
    int damage = 2; // Damage dealt by the fireball
    
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyScript enemyScript = collision.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                // Handle player collision with fireball
                enemyScript.health -= damage; // Assuming TakeDamage is a method in BasePlayerScript
            }
        }
    }
}
