using UnityEngine;

public class SlopeScript : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovementScript playerScript = collision.gameObject.GetComponent<PlayerMovementScript>();
            if (playerScript != null)
            {
                // Disable jumping, dashing, and moving up the slope
                playerScript.canMoveUpSlope = false;
                playerScript.canJump = false;
                playerScript.canDash = false;
            }
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovementScript playerScript = collision.gameObject.GetComponent<PlayerMovementScript>();
            if (playerScript != null)
            {
                // Re-enable abilities when leaving the slope
                playerScript.canMoveUpSlope = true;
                playerScript.canJump = true;
                playerScript.canDash = true;
            }
        }
    }
}