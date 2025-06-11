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
        animator = GetComponent<Animator>();
        canChase = false; // Initially not chasing the player
        Initialize();
        health = 3f; // Health value

        attackRange = 1.5f; // Set the attack range for melee attacks
        attackCooldown = 1.5f; // Set the cooldown time between attacks
        attackDamage = 1; // Damage dealt by the enemy's attack

        returnSpeed = 1f; // Speed at which the enemy returns to its starting position
        waitBeforeReturn = 5f; // Time to wait before returning to starting position after chasing

        rayDistance = 10f; // Example ray distance for detecting the player
        chaseSpeed = 2f; // Example chase speed
        maxChaseDistance = 15f; // Maximum distance to chase the player  

        scoreGain = 300;  
    }

    void Update()
    {
        //set health in animator to health value
        animator.SetInteger("Health", (int)health);
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x)); // Set the speed parameter for the animator
        CheckIfDead();
        if(health > 0){
            DetermineSpriteDirection();
            CheckForChase();
            Chase();
            CheckForAttack();
        }
    }

    protected override IEnumerator PerformAttack()
    {
        // Debug.Log("Performing melee attack on player");
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonAttack"))
        {
            yield return null;
        }

        // Now wait until the animation finishes
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonAttack") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        // Debug.Log("Melee attack wind-up complete, executing attack");
        // Perform the melee attack logic
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= attackRange)
        {
            Debug.Log("Player is within attack range, dealing damage.");
            playerScript.animator.SetTrigger("Hurt"); // Trigger the hit animation on the player
            //Since this is a melee attack, we can directly apply damage to the player
            playerScript.lives -= attackDamage;
        }
        else
        {
            Debug.Log("Player is out of attack range, no damage dealt.");
        }

        animator.SetBool("Attacking", false); // Reset the attacking animation state
        yield return new WaitForSeconds(attackCooldown); // Wait for the cooldown before the next attack
        attackCoroutine = null; // Reset the attack coroutine
    }
}