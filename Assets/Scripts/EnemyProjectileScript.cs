using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileScript : MonoBehaviour
{
    public Coroutine timeToLiveCoroutine; // Coroutine to handle the projectile's behavior
    public float projectileTimeToLive = 5f; // Time in seconds before the projectile is destroyed
    bool deflected = false; // Flag to check if the projectile has been deflected

    void Start()
    {
        // Start the coroutine to handle the projectile's lifetime
        timeToLiveCoroutine = StartCoroutine(ProjectileTimeToLive());
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !deflected)
        {
            BasePlayerScript playerScript = collision.gameObject.GetComponent<BasePlayerScript>();
            PlayerKnightFormScript knightFormScript = collision.gameObject.GetComponent<PlayerKnightFormScript>();
            
            if (playerScript != null && knightFormScript != null && knightFormScript.isDeflecting)
            {
                // If the player is in knight form and deflecting, do not take damage
                Debug.Log("Player deflected the enemy projectile.");
                //Reverse the direction of the projectile, and have it check for collision with the enemy
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = -rb.velocity;
                    deflected = true; // Set the deflected flag to true
                    StopCoroutine(timeToLiveCoroutine); // Stop the time to live coroutine
                    timeToLiveCoroutine = StartCoroutine(ProjectileTimeToLive()); // Restart the coroutine to handle the new projectile behavior
                }
            }
            else
            {
                // Handle player collision with projectile
                playerScript.lives -= 1; // Assuming lives is a public variable in BasePlayerScript
                Debug.Log("Player hit by enemy projectile. Lives left: " + playerScript.lives);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy") && deflected)
        {
            // Handle collision with enemy
            EnemyScript enemyScript = collision.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.health -= 1; // Assuming health is a public variable in EnemyScript
                Debug.Log("Enemy hit by projectile. Health left: " + enemyScript.health);
            }
        }
        Destroy(gameObject); // Destroy the projectile on collision
    }

    private IEnumerator ProjectileTimeToLive()
    {
        yield return new WaitForSeconds(projectileTimeToLive); // Wait for 5 seconds before destroying the projectile
        Destroy(gameObject); // Destroy the projectile after its lifetime ends
    }
}
