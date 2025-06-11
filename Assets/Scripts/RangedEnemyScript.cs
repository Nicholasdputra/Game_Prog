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
        animator = GetComponent<Animator>();
        canChase = false; // Initially not chasing the player
        Initialize();
        health = 1f; // Health value

        attackRange = 10f; // Set the attack range for ranged attacks
        attackCooldown = 3f; // Set the cooldown time between attacks
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
        //set health in animator to health value
        animator.SetInteger("Health", (int)health);
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x)); // Set the speed parameter for the animator
        DetermineSpriteDirection();
        CheckIfDead();
        if(health > 0){
            CheckForChase();
            Chase();
            CheckForAttack();
        }
    }

    protected override IEnumerator PerformAttack()
    {
        // Debug.Log("Performing attack on player");
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonAttack"))
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.05f);

        // Now perform the attack
        // Debug.Log("Performing ranged attack on player at frame 66");
        FireProjectile();
        yield return new WaitForSeconds(0.45f);
        animator.SetBool("Attacking", false);
        yield return new WaitForSeconds(attackCooldown); // Wait for the cooldown before the next attack
        attackCoroutine = null; // Reset the attack coroutine
    }

    void FireProjectile()
    {
        Debug.Log("Shooting");

        // Calculate direction from enemy to player (on the X axis only for horizontal shots, or use full vector for aiming)
        Collider2D playerCol = player.GetComponent<Collider2D>();
        Vector2 targetPos = playerCol != null ? playerCol.bounds.center : (Vector2)player.transform.position;

        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

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
