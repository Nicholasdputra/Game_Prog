using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyScript : EnemyScript
{
    public GameObject projectilePrefab; // Prefab for the projectile
    public int projectileSpeed = 20;
    public int shootingCooldown; // Cooldown time in seconds for firing
    public bool canShoot; // Flag to check if fireball can be fired

    // Start is called before the first frame update
    void Start()
    {
        canChase = false; // Initially not chasing the player
        Initialize();
        health = 1f; // Health value

        attackWindUp = 1f; // Time before the attack is executed
        attackRange = 10f; // Set the attack range for ranged attacks
        attackCooldown = 1f; // Set the cooldown time between attacks
        attackDamage = 1; // Damage dealt by the enemy's attack

        returnSpeed = 1f; // Speed at which the enemy returns to its starting position
        waitBeforeReturn = 5f; // Time to wait before returning to starting position after chasing

        rayDistance = 10f; // Example ray distance for detecting the player
        chaseSpeed = 4f; // Example chase speed
        maxChaseDistance = 20f; // Maximum distance to chase the player    
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfDead();
        CheckForChase();
        Chase();
        CheckForAttack();
    }

    protected override IEnumerator PerformAttack()
    {
        Debug.Log("Performing ranged attack on player");
        yield return new WaitForSeconds(attackWindUp);
        Debug.Log("Ranged attack wind-up complete, executing attack");

        // Perform the ranged attack logic
        FireProjectile();
        yield return new WaitForSeconds(attackCooldown); // Wait for the cooldown before the next attack

        attackCoroutine = null; // Reset the attack coroutine
    }

    void FireProjectile()
    {
        Debug.Log("Shooting");

        // Calculate direction from enemy to player (on the X axis only for horizontal shots, or use full vector for aiming)
        Vector2 direction = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;

        // Optionally clamp Y if you want to limit vertical angle
        direction.y = Mathf.Clamp(direction.y, -0.5f, 0.5f);
        direction = direction.normalized;

        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        // Get the Rigidbody2D
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        
        // Add force instead of setting velocity (Impulse mode for instant speed)
        projectileRb.AddForce(direction * projectileSpeed, ForceMode2D.Impulse);

        Debug.Log("Projectile force applied: " + (direction * projectileSpeed));
    }
}
