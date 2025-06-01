using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{
    int damage = 3; // Damage dealt by the fireball

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Fireball collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Fireball hit an enemy: " + collision.gameObject.name);
            EnemyScript enemyScript = collision.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                Debug.Log("Script isnt null, applying damage.");
                // Handle player collision with fireball
                enemyScript.health -= damage; // Assuming TakeDamage is a method in BasePlayerScript
            }
        }

        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject); // Destroy the fireball on collision
        }
    }
}