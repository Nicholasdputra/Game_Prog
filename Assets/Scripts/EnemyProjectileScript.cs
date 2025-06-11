using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileScript : MonoBehaviour
{
    public Coroutine timeToLiveCoroutine; // Coroutine to handle the projectile's behavior
    public float projectileTimeToLive = 5f; // Time in seconds before the projectile is destroyed

    void Start()
    {
        // Start the coroutine to handle the projectile's lifetime
        timeToLiveCoroutine = StartCoroutine(ProjectileTimeToLive());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if
        (
            (other.gameObject.CompareTag("EnemyProjectile") && gameObject.CompareTag("EnemyProjectile")) ||
            (other.gameObject.CompareTag("PlayerReflectedProjectile") && gameObject.CompareTag("PlayerReflectedProjectile")) 
        )
        {
            Debug.Log("Projectile collided with another projectile of the same type. Nothing happens.");
            return; // Ignore collisions between enemy projectiles or player-reflected projectiles
        }

        // Ignore enemy projectile hitting enemy, or reflected projectile hitting player
        if
        (
            (other.gameObject.CompareTag("Enemy") && gameObject.CompareTag("EnemyProjectile")) ||
            (other.gameObject.CompareTag("Player") && gameObject.CompareTag("PlayerReflectedProjectile"))
        )
        {
            Debug.Log("Projectile hit its own side. Nothing happens.");
            return;
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            BasePlayerScript playerScript = other.gameObject.GetComponent<BasePlayerScript>();
            PlayerKnightFormScript knightFormScript = other.gameObject.GetComponent<PlayerKnightFormScript>();
            
            if (playerScript != null && knightFormScript != null && knightFormScript.isDeflecting)
            {
                // If the player is in knight form and deflecting, do not take damage
                Debug.Log("Player deflected the enemy projectile.");
                // Change the tag of the projectile to indicate it has been deflected
                gameObject.tag = "PlayerReflectedProjectile"; // Change the tag to indicate it has been deflected
                //Reverse the direction of the projectile, and have it check for collision with the enemy
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 temp = rb.velocity; // Store the current velocity
                    rb.velocity = Vector2.zero; // Stop the projectile
                    rb.velocity = -temp; // Restore the velocity to reverse direction
                    StopCoroutine(timeToLiveCoroutine); // Stop the time to live coroutine
                    timeToLiveCoroutine = StartCoroutine(ProjectileTimeToLive()); // Restart the coroutine to handle the new projectile behavior
                }
            }
            else
            {
                // Handle player collision with projectile
                playerScript.lives -= 1; // Assuming lives is a public variable in BasePlayerScript
                if (playerScript.playerForm == 0)
                {
                    playerScript.animator.SetTrigger("Hurt");
                }
                Debug.Log("Player hit by enemy projectile. Lives left: " + playerScript.lives);
                AudioManager.instance.PlaySoundEffect(AudioManager.instance.hitSound); // Play hit sound effect
                Destroy(gameObject); // Destroy the projectile on collision
            }
        }
        else if (other.gameObject.CompareTag("Enemy") && gameObject.CompareTag("PlayerReflectedProjectile"))
        {
            // Handle collision with enemy
            EnemyScript enemyScript = other.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                enemyScript.health -= 1; // Assuming health is a public variable in EnemyScript
                if (enemyScript.health > 0)
                {
                    enemyScript.animator.SetTrigger("Hurt"); // Trigger the hurt animation
                }
                
                Debug.Log("Enemy hit by projectile. Health left: " + enemyScript.health);
            }
            Destroy(gameObject); // Destroy the projectile on collision
        }
        else
        {
            Debug.Log("Projectile collided with an object that is not the player or an enemy.");
            Destroy(gameObject); // Destroy the projectile on collision with objects
        }
    }

    private IEnumerator ProjectileTimeToLive()
    {
        yield return new WaitForSeconds(projectileTimeToLive); // Wait for 5 seconds before destroying the projectile
        Destroy(gameObject); // Destroy the projectile after its lifetime ends
    }
}
