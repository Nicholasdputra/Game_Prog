using UnityEngine;
using System.Collections;
// using System.Collections.Generic;

public abstract class EnemyScript : MonoBehaviour
{
    // Enemy-related variables
    public float health; // Health of the enemy
    public Rigidbody2D rb; // Reference to the Rigidbody2D component for physics interactions

    // Player-related variables
    public GameObject player; // Reference to the player GameObject
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
    protected abstract IEnumerator PerformAttack();

    // Chasing-related variables
    public float waitBeforeReturn; // Time to wait before returning to the starting position after chasing
    public Coroutine waitBeforeReturnCoroutine; // Coroutine for waiting before returning

    // Returning to starting position variables
    public float returnSpeed; // Speed at which the enemy returns to its starting position
    public Coroutine returnCoroutine; // Coroutine for returning to the starting position
    public Vector2 startingPosition; // Starting position of the enemy

    protected void Initialize()
    {
        attackCoroutine = null; // Initialize attack coroutine to null
        waitBeforeReturnCoroutine = null; // Initialize wait coroutine to null
        returnCoroutine = null; // Initialize return coroutine to null
        isChasing = false; // Initially not chasing the player
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the enemy
        startingPosition = transform.position; // Set the starting position of the enemy
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player object in the scene
        canChase = false; // Initially not chasing the player   
    }

    protected void Chase()
    {
        if (canChase && player != null)
        {
            // Debug.Log("Chasing player");
            isChasing = true;

            // Calculate the direction towards the player and move the enemy towards the player
            playerDirection = (player.transform.position - transform.position).normalized;
            // Debug.Log("Player is within chase distance, moving towards player");
            float newX = rb.position.x + playerDirection.x * chaseSpeed * Time.deltaTime;
            Vector2 newPos = new Vector2(newX, rb.position.y);

            rb.MovePosition(newPos);
            transform.position = new Vector2(newPos.x, newPos.y);

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
        if (playerIsntInRange && isChasing)
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
        if (isPlayerInRange)
        {
            // If the enemy is within attack range, start the attack coroutine if not already running
            canChase = false; // Stop chasing when in attack range
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
                attackCoroutine = StartCoroutine(PerformAttack());
            }
        }
        else
        {
            // If the enemy is not in attack range, stop the attack coroutine if it is running
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
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
}