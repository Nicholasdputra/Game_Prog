using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class MeeleeEnemyScript : EnemyScript
{
    // MeeleeEnemyScript inherits from EnemyScript and implements specific behavior for a melee enemy
   
    void Start()
    {
        canChase = false; // Initially not chasing the player
        Initialize();
        health = 3f; // Example health value

        attackRange = 1.5f; // Set the attack range for melee attacks
        attackCooldown = 1f; // Set the cooldown time between attacks

        returnSpeed = 1f; // Speed at which the enemy returns to its starting position
        waitBeforeReturn = 5f; // Time to wait before returning to starting position after chasing

        rayDistance = 10f; // Example ray distance for detecting the player
        chaseSpeed = 2f; // Example chase speed
        maxChaseDistance = 15f; // Maximum distance to chase the player    

    }

    void Update()
    {
        if (player != null)
            playerDirection = (player.transform.position - transform.position).normalized;

        CheckForChase();
        Chase();
        CheckForAttack();
    }

    void CheckForChase()
    {
        // We need to exclude the Enemy layer from the raycast to avoid detecting other enemies
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int mask = ~(1 << enemyLayer); // Exclude Enemy layer

        // Perform a circle overlap to detect the player within the specified ray distance
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rayDistance);
        // Debug.Log($"Detected {hits.Length} colliders within {rayDistance} units.");
        bool playerDetected = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Debug.Log($"Detected collider: {hit.name} with tag {hit.tag}");
                Vector2 directionToPlayer = (hit.transform.position - transform.position).normalized;
                float distanceToPlayer = Vector2.Distance(transform.position, hit.transform.position);

                RaycastHit2D rayHit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, mask);

                Debug.DrawLine(transform.position, rayHit.point, Color.green);
                // Debug.Log($"Raycast hit: {rayHit.collider?.name} at distance {rayHit.distance}");
                if (rayHit.collider != null && rayHit.collider.CompareTag("Player"))
                {
                    playerDetected = true;
                    break; // No need to check further
                }
            }
        }
        // Debug.Log($"Player detected: {playerDetected}");
        if (Vector2.Distance(transform.position, startingPosition) < maxChaseDistance)
        {
            canChase = playerDetected;        
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rayDistance);
    }

    protected override IEnumerator PerformAttack()
    {
        Debug.Log("Performing melee attack on player");
        // // Perform the melee attack logic
        // if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackRange)
        // {
        //     // Example attack logic: reduce player's health
        //     PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //     if (playerHealth != null)
        //     {
        //         playerHealth.TakeDamage(1); // Reduce player's health by 1
        //     }
        // }
        yield return new WaitForSeconds(attackCooldown); // Wait for the cooldown before the next attack
    }
}
