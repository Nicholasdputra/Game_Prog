using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerKnightFormScript : MonoBehaviour
{
    public GameObject shieldIcon;
    public GameObject player;
    public BasePlayerScript playerScript; // Reference to the PlayerMovementScript
    public Animator animator;

    // Deflect properties
    // We're not actually implementing a deflect feature here in this script, rather on OnCollisionEnter with projectiles to check if we should take away lives
    public bool isDeflecting = false; // Flag to check if the player is deflecting
    public int deflectDuration = 3; // Duration of the deflect action in seconds
    public int deflectCooldown = 5; // Cooldown time in seconds for deflect action

    // Sword attack properties
    public Coroutine swordSwingCoroutine; // Coroutine for sword swing
    public bool canSwingSword = true; // Flag to check if the player can swing the sword
    public float arcAngle = 120f;
    public int swordDamage = 1;
    public int swordSwingCooldown = 1; // Cooldown time in seconds for sword swing
    public float attackRange = 1.5f; // Range of the sword attack
    
    void OnEnable()
    {
        // Initialize or reset variables when the player enters knight form
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<BasePlayerScript>();
        animator = player.GetComponent<Animator>();
        isDeflecting = false;
        canSwingSword = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for input to trigger sword swing
        if (Input.GetKeyDown(KeyCode.E) && canSwingSword)
        {
            isDeflecting = false; // Reset deflecting state
            Debug.Log("Sword swing action initiated.");
            animator.SetBool("Deflecting", false); // Set the deflecting animation state
            animator.SetBool("Attacking", true); // Set the attacking animation state
            Debug.Log("Attacking animation state set to true.");
            swordSwingCoroutine = StartCoroutine(SwordSwing());
        }

        if (Input.GetKey(KeyCode.Q) && swordSwingCoroutine == null)
        {
            animator.SetBool("Attacking", false); // Reset the attacking animation state
            animator.SetBool("Deflecting", true); // Set the deflecting animation state
            shieldIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            isDeflecting = true;
            canSwingSword = false; // Disable sword swinging while deflecting
        } 
        else
        {
            animator.SetBool("Deflecting", false); // Reset the deflecting animation state
            
            shieldIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            if (swordSwingCoroutine == null)
            {
                canSwingSword = true; // Disable sword swinging while deflecting    
            }
            isDeflecting = false;
        }
    }

    // Add this method to PlayerKnightFormScript.cs or EnemyScript.cs
    private IEnumerator FlashRed(SpriteRenderer spriteRenderer, float duration = 0.1f)
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = originalColor;
    }


    public IEnumerator SwordSwing()
    {
        canSwingSword = false; // Disable sword swinging

        // Wait until the animator enters the "Attack 1" state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 1"))
        {
            yield return null;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
        {
            yield return null;
        }

        // Determine the attack direction based on your direction variable
        Vector2 attackDir;
        Vector2 arcStartDir;
        if(playerScript.direction == 1)
        {
            arcStartDir = Quaternion.Euler(0, 0, 30) * Vector2.up; // 30 degrees from up
            attackDir = Vector2.right; // Right-facing player
        }
        else
        {
            // 30 degrees from up to the left
            arcStartDir = Quaternion.Euler(0, 0, -30) * Vector2.up; // 30 degrees from up to the left
            attackDir = Vector2.left; // Left-facing player
        }

        // Check for enemies in range and apply damage in an arc
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Vector2 toEnemy = (enemy.transform.position - transform.position).normalized;
                // Detemine the angle from the arc start direction to the enemy
                float angleToEnemy = Vector2.SignedAngle(arcStartDir, toEnemy);

                bool inArc = false;
                if (playerScript.direction == 1)
                {
                    // Right: angle between 0 and -arcAngle
                    if (angleToEnemy <= 0 && angleToEnemy >= -arcAngle)
                    {
                        inArc = true;
                    }
                    else
                    {
                        inArc = false;
                    }
                }
                else
                {
                    // Left: angle between 0 and +arcAngle
                    if (angleToEnemy >= 0 && angleToEnemy <= arcAngle)
                    {
                        inArc = true;
                    }
                    else
                    {
                        inArc = false;
                    }
                }

                // Only hit enemies within the arc
                if (inArc)
                {
                    EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
                    enemyScript.health -= swordDamage;
                    if (enemy.GetComponent<SpriteRenderer>() != null && enemyScript.health > 0)
                    {
                        enemyScript.animator.SetTrigger("Hurt");
                        StartCoroutine(FlashRed(enemy.GetComponent<SpriteRenderer>()));
                    }
                }
            }
        }

        animator.SetBool("Attacking", false); // Reset the attacking animation state
        Debug.Log("Attacking animation state set to false.");

        canSwingSword = true; // Re-enable sword swinging
        Debug.Log("Sword swing action is ready again.");
        swordSwingCoroutine = null; // Reset the coroutine reference
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Set the arc start angle based on direction
        float startAngle = 0f;
        float sweepAngle = arcAngle;
        if (playerScript != null && playerScript.direction == 1)
        {
            // Right: start at +30, sweep clockwise (negative)
            startAngle = 30f;
            sweepAngle = -arcAngle;
        }
        else
        {
            // Left: start at -30, sweep counterclockwise (positive)
            startAngle = -30f;
            sweepAngle = arcAngle;
        }

        int segments = 20;
        Gizmos.color = Color.red;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            float angle1 = startAngle + sweepAngle * t1;
            float angle2 = startAngle + sweepAngle * t2;

            Vector2 dir1 = Quaternion.Euler(0, 0, angle1) * Vector2.up;
            Vector2 dir2 = Quaternion.Euler(0, 0, angle2) * Vector2.up;
            Gizmos.DrawLine(transform.position + (Vector3)(dir1 * attackRange), transform.position + (Vector3)(dir2 * attackRange));
        }

        // Draw the start and end lines of the arc
        Vector2 arcStartDir = Quaternion.Euler(0, 0, startAngle) * Vector2.up;
        Vector2 arcEndDir = Quaternion.Euler(0, 0, startAngle + sweepAngle) * Vector2.up;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(arcStartDir * attackRange));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(arcEndDir * attackRange));
    }
}
