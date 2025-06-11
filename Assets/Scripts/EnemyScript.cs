using UnityEngine;
using System.Collections;
// using System.Collections.Generic;

public abstract class EnemyScript : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component for animations

    // Enemy-related variables
    public float health; // Health of the enemy
    public Rigidbody2D rb; // Reference to the Rigidbody2D component for physics interactions

    // Player-related variables
    public GameObject player; // Reference to the player GameObject
    public BasePlayerScript playerScript; // Reference to the player's script for accessing player data
    public Vector2 playerDirection; // Direction towards the player
    public float rayDistance; // Distance for raycasting to detect the player
    public bool isChasing = false; // Flag to indicate if the enemy is currently chasing the player
    public bool canChase; // Flag to indicate if the enemy is currently chasing the player
    public float chaseSpeed; // Speed at which the enemy chases the player
    public float maxChaseDistance; // Maximum distance the enemy will chase the player before returning

    // Attack-related variables
    public float attackRange; // Range within which the enemy can attack
    public float attackCooldown; // Cooldown time between attacks
    public Coroutine attackCoroutine; // Coroutine for handling attack timing
    protected abstract IEnumerator PerformAttack(); // Abstract method to be implemented by derived classes for performing the attack
    public int attackDamage; // Damage dealt by the enemy's attack

    // Chasing-related variables
    public float waitBeforeReturn; // Time to wait before returning to the starting position after chasing
    public Coroutine waitBeforeReturnCoroutine; // Coroutine for waiting before returning

    // Returning to starting position variables
    public float returnSpeed; // Speed at which the enemy returns to its starting position
    public Coroutine returnCoroutine; // Coroutine for returning to the starting position
    public Vector2 startingPosition; // Starting position of the enemy

    protected void Initialize()
    {
        // Initialize the enemy script
        attackCoroutine = null; // Initialize attack coroutine to null
        waitBeforeReturnCoroutine = null; // Initialize wait coroutine to null
        returnCoroutine = null; // Initialize return coroutine to null
        isChasing = false; // Initially not chasing the player
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the enemy
        startingPosition = transform.position; // Set the starting position of the enemy
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player object in the scene
        playerScript = player.GetComponent<BasePlayerScript>(); // Get the player's script component
        canChase = false; // Initially not chasing the player   
    }

    protected void CheckForChase()
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
                Collider2D playerCol = hit.GetComponent<Collider2D>();
                Vector2 playerTarget = playerCol != null ? playerCol.bounds.center : hit.transform.position;

                Vector2 directionToPlayer = (playerTarget - (Vector2)transform.position).normalized;
                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget);

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


    //For debugging purposes, draw a wire sphere in the scene view to visualize the ray distance, only if the object is selected though
    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.cyan;
    //     Gizmos.DrawWireSphere(transform.position, rayDistance);
    // }

    protected void Chase()
    {
        if (canChase && player != null 
        && !animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonHurt") 
        && !animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonHurt"))
        {
            // Debug.Log("Chasing player");
            // Stop chasing if already in attack range
            if (Vector2.Distance(transform.position, player.transform.position) <= attackRange)
            {
                canChase = false;
                return;
            }
            isChasing = true;

            // Calculate the direction towards the player and move the enemy towards the player
            playerDirection = (player.transform.position - transform.position).normalized;
            // Debug.Log("Player is within chase distance, moving towards player");
            Vector2 force = new Vector2(playerDirection.x * chaseSpeed, 0f);
            rb.AddForce(force, ForceMode2D.Force);
            float maxSpeed = Mathf.Abs(chaseSpeed); // Or set a separate maxSpeed variable if you want

            // Clamp X velocity and instantly flip direction if needed, but keep Y velocity (gravity)
            float currentX = rb.velocity.x;
            float desiredX = playerDirection.x * maxSpeed;

            // If changing direction, snap to desiredX; otherwise, clamp
            if (Mathf.Abs(currentX) > 0.1f && Mathf.Sign(currentX) != Mathf.Sign(playerDirection.x))
            {
                rb.velocity = new Vector2(desiredX, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Clamp(currentX, -maxSpeed, maxSpeed), rb.velocity.y);
            }

            // Check if the enemy has reached the maximum chase distance
            if (Vector2.Distance(transform.position, startingPosition) > maxChaseDistance)
            {
                // Debug.Log("Max chase distance reached, starting wait coroutine");
                canChase = false; // Stop chasing the player
                CheckForWaitAndReturn();
                isChasing = false; // Reset after chase ends
            }
        }
        else
        {
            if ((Vector2)transform.position != (Vector2)startingPosition)
            {
                // Debug.Log("Player is not in chase range, checking for wait and return");
                CheckForWaitAndReturn();
            }
        }
    }

    void CheckForWaitAndReturn()
    {
        bool playerIsntInRange = Vector2.Distance(transform.position, player.transform.position) > attackRange;
        // Debug.Log("Player is not in attack range: " + playerIsntInRange);
        // If player is not in attack range, start wait coroutine
        if (playerIsntInRange && isChasing && !animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonHurt") 
        && !animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonHurt"))
        {
            if (waitBeforeReturnCoroutine == null)
            {
                // Debug.Log("Starting wait coroutine before returning to starting position");
                // Start the wait coroutine to return after a delay
                waitBeforeReturnCoroutine = StartCoroutine(WaitAndReturn());
            }
        }
    }

    protected IEnumerator WaitAndReturn()
    {
        // Debug.Log("Waiting before returning to starting position");
        float timer = 0f;
        while (timer < waitBeforeReturn)
        {
            // Debug.Log("Waiting for " + (waitBeforeReturn - timer) + " seconds before returning");
            // If player enters attack range, cancel return and resume chasing
            if (Vector2.Distance(transform.position, player.transform.position) <= attackRange ||
                canChase)
            {
                waitBeforeReturnCoroutine = null;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // After waiting, start returning to start position
        // Debug.Log("Wait time completed, returning to starting position");
        canChase = false;
        returnCoroutine = StartCoroutine(ReturnToStartingPosition());
        waitBeforeReturnCoroutine = null;
    }

    protected IEnumerator ReturnToStartingPosition() // Move the enemy back to its starting position
    {
        // Debug.Log("Returning to starting position");
        while (Vector2.Distance(transform.position, startingPosition) > 0.1f)
        {
            // Debug.Log("Moving towards starting position");
            Vector2 direction = (startingPosition - (Vector2)transform.position).normalized;
            float newX = rb.position.x + direction.x * chaseSpeed * Time.deltaTime;
            Vector2 newPos = new Vector2(newX, rb.position.y);
            rb.MovePosition(newPos);
            transform.position = new Vector2(newPos.x, newPos.y);
            yield return null; // Wait for the next frame

            // If player enters attack range, cancel return and resume chasing
            bool isPlayerInRange = Vector2.Distance(transform.position, player.transform.position) <= attackRange;
            // Debug.Log("Checking if player is within attack range during return: " + isPlayerInRange);
            //Draw line for attack range for debugging
            DrawDebugLine(transform.position, player.transform.position, Color.red, attackRange);

            if (isPlayerInRange || canChase)
            {
                canChase = true; // Resume chasing the player
                returnCoroutine = null;
                yield break;     // Just exit the coroutine
            }
        }
        ResetPosition(); // Reset position when close enough to starting point
        returnCoroutine = null;
    }

    void DrawDebugLine(Vector2 from, Vector2 to, Color color, float maxDistance)
    {
        Vector2 direction = (to - from).normalized;
        float distance = Mathf.Min(Vector2.Distance(from, to), maxDistance);
        Vector2 endPoint = from + direction * distance;
        Debug.DrawLine(from, endPoint, color);
    }

    protected void CheckForAttack()
    {
        // Debug.Log("Checking for attack");
        if (player == null) return; // If player is not set, exit

        // Check if the enemy can attack the player
        bool isPlayerInRange = Vector2.Distance(transform.position, player.transform.position) <= attackRange;
        // Debug.Log("Checking if player is within attack range: " + isPlayerInRange);
        if (isPlayerInRange && !animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonHurt") 
        && !animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonHurt"))
        {
            // If the enemy is within attack range, start the attack coroutine if not already running
            if (waitBeforeReturnCoroutine != null)
            {
                StopCoroutine(waitBeforeReturnCoroutine);
                waitBeforeReturnCoroutine = null;
            }
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }

            if (attackCoroutine == null)
            {
                // Debug.Log("Player is within attack range, starting attack coroutine");
                animator.SetBool("Attacking", true); // Set the attacking animation state
                attackCoroutine = StartCoroutine(PerformAttack());
            }
        }
    }

    protected void ResetPosition() // Reset the enemy's position and state
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        if (waitBeforeReturnCoroutine != null)
        {
            StopCoroutine(waitBeforeReturnCoroutine);
            waitBeforeReturnCoroutine = null;
        }
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        transform.position = startingPosition; // Reset the enemy's position to the starting point
        canChase = false; // Stop chasing when resetting position
    }
    
    protected void CheckIfDead()
    {
        // Check if the enemy's health is less than or equal to zero
        if (health <= 0)
        {
            StartCoroutine(PlayDeathAnimation());
        }
    }

    protected IEnumerator PlayDeathAnimation()
    {
        Debug.Log("Enemy is dead, playing death animation");
        rb.velocity = Vector2.zero; // Stop any movement
        StopAllCoroutines(); // Stop all coroutines to prevent further actions
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonDeath") &&
            !animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonDeath"))
        {
            yield return null; // Wait until the death animation state is reached
        }

        // Now wait until the animation finishes
        while ((animator.GetCurrentAnimatorStateInfo(0).IsName("MeeleeSkeletonDeath") 
        || animator.GetCurrentAnimatorStateInfo(0).IsName("RangedSkeletonDeath")) 
        && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        Destroy(gameObject);
    }

    protected void DetermineSpriteDirection()
    {
        // Determine the direction of the sprite based on the player's position
        if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
            if (directionToPlayer.x < 0)
            {
                // Player is to the left, flip the sprite
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                // Player is to the right, reset flip
                GetComponent<SpriteRenderer>().flipX = false;
            }
        }
    }
}