using System.Collections;
using UnityEngine;

public class BasePlayerScript : MonoBehaviour
{
    private GameManagerScript gameManager;
    public GameObject player;
    public PlayerKnightFormScript playerKnightFormScript;
    public PlayerFireFormScript playerFireFormScript;
    public Rigidbody2D rb;

    public int lives = 3; // Player's health
    public bool isGrounded;
    public int playerForm; // 0 = Knight, 1 = Fire (if i decide to add more forms later, I'll use this to track the current form)

    public Collider2D col;
    public int direction;
    [SerializeField] public float baseSpeed;
    float maxSpeed = 7f; // Set this to your desired max speed

    public bool canMoveUpSlope = true;
    public bool canJump = true;
    public bool canDash = true;

    [SerializeField] public float baseDashForce;
    public bool isDashing;
    private int dashcooldown = 0;
    public static int baseDashCooldown = 5;

    [SerializeField] public float baseJumpForce;
    public bool canDoubleJump;

    void Start()
    {
        dashcooldown = baseDashCooldown; // Initialize dash cooldown
        baseSpeed = 4f;
        baseDashForce = 7f;
        baseJumpForce = 6.5f;
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody2D>();
        col = player.GetComponent<Collider2D>();
        playerKnightFormScript = player.GetComponent<PlayerKnightFormScript>();
        playerFireFormScript = player.GetComponent<PlayerFireFormScript>();
        direction = 1;
        canDoubleJump = false;
        isGrounded = false;
        // Debug.Log("PlayerMovementScript started. GameManager instance: " + gameManager);
    }

    void Update()
    {
        // gameManager.playerData.positionInLevel[gameManager.currentLevel] = player.transform.position;
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

            rb.velocity = new Vector2(moveInput * baseSpeed, rb.velocity.y);

            if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            {
                rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
            }
        }

        // Grounded check using OverlapBox for a wide check
        Vector2 boxCenter = (Vector2)player.transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);
        Vector2 boxSize = new Vector2(col.bounds.size.x * 0.9f, 0.1f); // 90% of collider width, 0.1 height
        Collider2D groundHit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, LayerMask.GetMask("Platforms"));
        Debug.DrawLine(boxCenter - Vector2.right * boxSize.x / 2, boxCenter + Vector2.right * boxSize.x / 2, Color.green);

        isGrounded = groundHit != null;

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
                Debug.Log("Base Jump Force: " + baseJumpForce);
                rb.AddForce(new Vector2(0, baseJumpForce), ForceMode2D.Impulse);
                canJump = false; // Disable jump until grounded again
            }
            else if (canDoubleJump)
            {
                // Debug.Log("Attempting double jump");
                Debug.Log("Base Jump Force: " + baseJumpForce);
                rb.AddForce(new Vector2(0, baseJumpForce), ForceMode2D.Impulse);
                canDoubleJump = false;
            }
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            Debug.Log("Attempting to dash");
            canDash = false; // Disable dashing until cooldown is over
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
            StartCoroutine(EndDash());
        }

        // Change Player Form Using Number Keys
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            // Switch to Knight Form
            playerForm = 0; // Knight Form
            Debug.Log("Switched to Knight Form");
            playerKnightFormScript.enabled = true;
            playerFireFormScript.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            // Switch to Fire Form
            playerForm = 1; // Fire Form
            Debug.Log("Switched to Fire Form");
            player.GetComponent<PlayerFireFormScript>().enabled = true;
            player.GetComponent<PlayerKnightFormScript>().enabled = false;
        }

    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
        StartCoroutine(DashCooldown(dashcooldown));
    }

    private IEnumerator DashCooldown(int dashcooldown)
    {
        while (dashcooldown > 0)
        {
            Debug.Log("Dash Cooldown: " + dashcooldown);
            dashcooldown--;
            yield return new WaitForSeconds(1);
        }
        canDash = true; // Re-enable dashing after cooldown
        Debug.Log("Dash Ready");
        dashcooldown = baseDashCooldown; // Reset cooldown
    }

    
}