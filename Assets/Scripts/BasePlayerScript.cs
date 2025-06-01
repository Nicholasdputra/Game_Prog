using System.Collections;
using UnityEngine;

public class BasePlayerScript : MonoBehaviour
{
    private GameManagerScript gameManager;
    public GameObject player;
    public Rigidbody2D rb;

    public int lives = 3; // Player's health
    public bool isGrounded;

    public Collider2D col;
    public int direction;
    public float baseSpeed = 5f;

    public bool canMoveUpSlope = true;
    public bool canJump = true;
    public bool canDash = true;

    public float baseDashForce = 20f;
    public bool isDashing;
    private int dashcooldown = 0;
    public static int baseDashCooldown = 5;

    public float baseJumpForce = 5f;
    public bool canDoubleJump;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody2D>();
        col = player.GetComponent<Collider2D>();
        direction = 1;
        canDoubleJump = false;
        isGrounded = false;
        gameManager = GameManagerScript.Instance;
        // Debug.Log("PlayerMovementScript started. GameManager instance: " + gameManager);
    }

    void Update()
    {
        gameManager.playerData.positionInLevel[gameManager.currentLevel] = player.transform.position;
        Vector2 inputVelocity = rb.velocity;

        // Only allow movement if not dashing and moving up slope is allowed
        if (!isDashing && canMoveUpSlope)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            float movementMultiplier = 0f;

            if (moveInput > 0)
            {
                direction = 1;
                movementMultiplier = baseSpeed;
            }
            else if (moveInput < 0)
            {
                direction = -1;
                movementMultiplier = -baseSpeed;
            }
            else
            {
                // direction = 0;
                movementMultiplier = 0f;
            }

            rb.velocity = new Vector2(movementMultiplier, rb.velocity.y);
        }

        // Grounded check using Raycast
        Vector2 origin = (Vector2)player.transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.1f);
        Debug.DrawRay(origin, Vector2.down * 0.1f, Color.red);

        if (hit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Only reset canDoubleJump when landing
        if (isGrounded)
        {
            canJump = true;
            canDoubleJump = true;
        }

        // Jump & Double Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canJump && isGrounded)
            {
                // Debug.Log("Jumping from ground");
                rb.AddForce(new Vector2(0, baseJumpForce), ForceMode2D.Impulse);
                canJump = false; // Disable jump until grounded again
            }
            else if (canDoubleJump)
            {
                // Debug.Log("Attempting double jump");
                rb.AddForce(new Vector2(0, baseJumpForce), ForceMode2D.Impulse);
                canDoubleJump = false;
            }
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            if (direction == 1 && rb.velocity.x > 0)
            {
                isDashing = true;
                Debug.Log("Dashing Right");
                rb.AddForce(new Vector2(baseDashForce, 0), ForceMode2D.Impulse);
            }
            else if (direction == -1 && rb.velocity.x < 0)
            {
                isDashing = true;
                Debug.Log("Dashing Left");
                rb.AddForce(new Vector2(-baseDashForce, 0), ForceMode2D.Impulse);
            }
            StartCoroutine(DashCooldown());
            StartCoroutine(EndDash());
        }
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
    }

    private IEnumerator DashCooldown()
    {
        while (dashcooldown > 0)
        {
            Debug.Log("Dash Cooldown: " + dashcooldown);
            dashcooldown--;
            yield return new WaitForSeconds(1);
        }
        canDash = true; // Re-enable dashing after cooldown
        Debug.Log("Dash Ready");
    }
}