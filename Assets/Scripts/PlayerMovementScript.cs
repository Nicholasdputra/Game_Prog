using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D rb;
    public bool doubleJumped;
    public bool isGrounded;
    public bool isDashing;
    public Collider2D col;
    public int direction;
    private int dashcooldown = 0;
    public static int baseDashCooldown = 5;
    int maxBounces = 5;
    float skinWidth = 0.03f;
    public LayerMask layerMask;
    float maxSlopeAngle = 45f;
    private bool doubleJumpOnCooldown = false;
    public float doubleJumpCooldownTime = 0.3f;
    private bool wasGrounded = false;
    private float coyoteTime = 0.1f;
    private float coyoteTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Platforms");
        player = GameObject.FindGameObjectWithTag("Player");
        rb = player.GetComponent<Rigidbody2D>();
        col = player.GetComponent<Collider2D>();
        direction = 0;
        doubleJumped = false;
        isGrounded = false;
    }

    // Update is called once per frame
    void Update()
    {
        Bounds bounds;
        bounds = col.bounds;
        bounds.Expand(-skinWidth * 2);

        Vector2 inputVelocity = Vector2.zero;

        // Only allow movement if not dashing
        if (!isDashing)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                direction = 1;
                inputVelocity = new Vector2(5, rb.velocity.y);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                direction = -1;
                inputVelocity = new Vector2(-5, rb.velocity.y);
            }
            else if (Input.GetAxis("Horizontal") <= 0.01 && isGrounded)
            {
                direction = 0;
                inputVelocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                inputVelocity = rb.velocity;
            }

            // Pass movement delta (velocity * Time.deltaTime)
            Vector2 movementDelta = inputVelocity * Time.deltaTime;
            Vector2 newDelta = CollideAndSlide(
                movementDelta,
                rb.position,
                0,
                false,
                movementDelta
            );
            // Convert back to velocity for Rigidbody2D
            rb.velocity = new Vector2(newDelta.x / Time.deltaTime, rb.velocity.y);
        }

        //Grounded and Jump Bool Reset using Raycast
        Vector2 origin = (Vector2)player.transform.position + Vector2.down * (col.bounds.extents.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.1f);
        Debug.DrawRay(origin, Vector2.down * 0.1f, Color.red);

        Vector2 groundNormal = Vector2.up; // Default to up
        if (hit.collider != null)
        {
            isGrounded = true;
            doubleJumped = false;
            groundNormal = hit.normal; // Store the normal
            coyoteTimer = coyoteTime; // Reset coyote time
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector2.up;
            coyoteTimer -= Time.deltaTime;
        }
        bool canJump = coyoteTimer > 0f;

        // Only reset doubleJumped when landing
        if (isGrounded && !wasGrounded)
        {
            doubleJumped = false;
        }
        wasGrounded = isGrounded;

        float slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
        bool onFlatGround = slopeAngle < 1f; // Tweak the angle tolerance as needed

        // Jump & Double Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canJump && isGrounded && onFlatGround)
            {
                Debug.Log("Jumping from ground");
                // First jump
                rb.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
                doubleJumped = false; // Reset double jump on ground
                coyoteTimer = 0f; // Reset coyote time
            }
            else if (!isGrounded && !doubleJumped && !doubleJumpOnCooldown)
            {
                Debug.Log("Attempting double jump");
                // Double jump (in air, not already double-jumped)
                rb.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
                doubleJumped = true;
                StartCoroutine(DoubleJumpCooldown());
            }
        }

        //Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashcooldown == 0 && onFlatGround)
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

            dashcooldown = baseDashCooldown;
            StartCoroutine(DashCooldown());
            StartCoroutine(EndDash());
        }

    }

    //Dash End
    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
    }

    //Dash Cooldown
    private IEnumerator DashCooldown()
    {
        while (dashcooldown > 0)
        {
            Debug.Log("Dash Cooldown: " + dashcooldown);
            dashcooldown--;
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator DoubleJumpCooldown()
    {
        doubleJumpOnCooldown = true;
        yield return new WaitForSeconds(doubleJumpCooldownTime);
        doubleJumpOnCooldown = false;
    }

    // Collide and Slide movement with capsule cast and sliding on slopes
    private Vector2 CollideAndSlide(Vector2 velocity, Vector2 position, int depth, bool gravityPass, Vector2 velInit)
    {
        // Prevent infinite recursion
        if (depth >= maxBounces)
        {
            return Vector2.zero;
        }
        // Calculate cast distance (movement + skin width buffer)
        float dist = velocity.magnitude + skinWidth;

        // Perform capsule cast
        Vector2 capsuleSize = new Vector2(col.bounds.size.x, col.bounds.size.y);

        // Perform a capsule cast in the direction of movement
        RaycastHit2D hit = Physics2D.CapsuleCast(position, capsuleSize, CapsuleDirection2D.Vertical, 0, velocity.normalized, dist, layerMask);


        if (hit.collider != null)
        {
            // Debug.Log("Hit something: " + hit.collider.name);
            // Move up to the collision point, minus skin width
            Vector2 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
            // Calculate the remaining velocity after snapping to the surface
            Vector2 leftover = velocity - snapToSurface;
            // Find the angle between the surface normal and up (to check slope)
            float angle = Vector2.Angle(hit.normal, Vector2.up);

            // If the snap is very small, treat as zero to avoid jitter
            if (snapToSurface.magnitude <= skinWidth)
            {
                snapToSurface = Vector2.zero;
                leftover = Vector2.zero;
            }

            // If the angle is less than the max slope angle, allow sliding
            if (angle <= maxSlopeAngle)
            {
                Debug.Log("Collided with slope, angle: " + angle);
                // If the slope is walkable
                if (gravityPass)
                {
                    // If gravity pass is true, just snap to surface
                    return snapToSurface;
                }
                // Project leftover velocity onto the surface for sliding
                leftover = ProjectAndScale(leftover, hit.normal);
            }
            else
            {
                // If the slope is too steep, still slide along it
                leftover = ProjectAndScale(leftover, hit.normal);
            }

            // Recursively handle further collisions with the leftover velocity
            return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1, gravityPass, velInit);
        }

        // No collision, move as intended
        return velocity;
    }

    private float slopeSpeedMultiplier = 0.8f;
    // Projects the velocity onto the plane defined by the surface normal (for sliding)
    private Vector2 ProjectAndScale(Vector2 velocity, Vector2 normal)
    {
        // Project velocity onto the surface
        Vector2 projected = velocity - Vector2.Dot(velocity, normal) * normal;
        
        // Check if moving up a slope (projected has positive y and normal is not up)
        if (projected.y > 0 && Mathf.Abs(normal.y) < 0.99f)
        {
            // Reduce speed when moving up a slope
            // Debug.Log("Moving up a slope, reducing speed");
            return projected.normalized * velocity.magnitude * slopeSpeedMultiplier;
        }
        else
        {
            // Debug.Log("Sliding on a surface, maintaining speed");
            return projected.normalized * velocity.magnitude;
        }
    }
}