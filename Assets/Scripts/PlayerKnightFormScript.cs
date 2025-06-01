using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnightFormScript : MonoBehaviour
{
    //To Do:
    //We can maybe adjust the player movement script to allow for a different speed or jump height

    public GameObject player;
    public BasePlayerScript playerScript; // Reference to the PlayerMovementScript
    
    // Deflect properties
    // We're not actually implementing a deflect feature here in this script, rather on OnCollisionEnter with projectiles to check if we should take away lives
    public bool canDeflect = true; // Flag to check if the player can deflect,
    public bool isDeflecting = false; // Flag to check if the player is deflecting
    public int deflectDuration = 3; // Duration of the deflect action in seconds
    public int deflectCooldown = 5; // Cooldown time in seconds for deflect action

    // Sword attack properties
    public bool canSwingSword = true; // Flag to check if the player can swing the sword
    public int swordDamage = 1;
    public int swordSwingCooldown = 1; // Cooldown time in seconds for sword swing
    public float attackRange = 1.5f; // Range of the sword attack

    void OnEnable()
    {
        // Initialize or reset variables when the player enters knight form
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<BasePlayerScript>();
        isDeflecting = false;
        canDeflect = true;
        canSwingSword = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for input to trigger sword swing
        if (Input.GetKeyDown(KeyCode.E) && canSwingSword) // Assuming space key is used for sword swing
        {
            SwordSwing();
        }

        if (Input.GetKeyDown(KeyCode.Q) && canDeflect && !isDeflecting) 
        {
            isDeflecting = true;
            StartCoroutine(DeflectCoroutine());
        }
    }

    void SwordSwing()
    {
        // Implement sword swinging logic here
        // This could involve playing an animation, checking for collisions, etc.
        canSwingSword = false; // Disable sword swinging until cooldown is over
        // Perform the sword swing action
        // For example, check for enemies in range and apply damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                enemy.GetComponent<EnemyScript>().health -= swordDamage;
            }
        }
        StartCoroutine(SwordSwingCooldown()); // Start the sword swing cooldown coroutine

    }

    public IEnumerator SwordSwingCooldown()
    {
        Debug.Log("Sword swing action ended, entering cooldown.");
        canSwingSword = false; // Disable sword swinging, just as a backup for the one in SwordSwing()
        yield return new WaitForSeconds(swordSwingCooldown); // Wait for the cooldown period
        canSwingSword = true; // Re-enable sword swinging
        Debug.Log("Sword swing action is ready again.");
    }

    public IEnumerator DeflectCoroutine()
    {
        canDeflect = false; // Disable the ability for player to stack deflects
        Debug.Log("Deflect action started.");
        yield return new WaitForSeconds(deflectDuration); // Wait for the deflect duration
        isDeflecting = false; // Reset deflecting state
        Debug.Log("Deflect action ended, entering cooldown.");
        yield return new WaitForSeconds(deflectCooldown); // Wait for the cooldown period
        canDeflect = true; // Re-enable deflecting
        Debug.Log("Deflect action is ready again.");
    }
}
