using System.Collections;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;

    public bool isGrounded;

    public Collider2D col;
    public int direction;

    public bool canMoveUpSlope = true;
    public bool canJump = true;
    public bool canDash = true;

    public bool isDashing;
    private int dashcooldown = 0;
    public static int baseDashCooldown = 5;

    public bool canDoubleJump;
    public float doubleJumpCooldownTime = 0.3f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody2D>();
        col = player.GetComponent<Collider2D>();
        direction = 1;
        canDoubleJump = false;
        isGrounded = false;
    }

    void Update()
    {
        Vector2 inputVelocity = rb.velocity;

        // Only allow movement if not dashing and moving up slope is allowed
        if (!isDashing && canMoveUpSlope)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            float movementMultiplier = 0f;
            if (moveInput > 0)
            {
                direction = 1;
                movementMultiplier = 5f;
            }
            else if (moveInput < 0)
            {
                direction = -1;
                movementMultiplier = -5f;
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
                Debug.Log("Jumping from ground or coyote time");
                rb.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
                canJump = false; // Disable jump until grounded again
            }
            else if (canDoubleJump)
            {
                Debug.Log("Attempting double jump");
                rb.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
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
                rb.AddForce(new Vector2(20, 0), ForceMode2D.Impulse);
            }
            else if (direction == -1 && rb.velocity.x < 0)
            {
                isDashing = true;
                Debug.Log("Dashing Left");
                rb.AddForce(new Vector2(-20, 0), ForceMode2D.Impulse);
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