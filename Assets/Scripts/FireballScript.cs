using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{
    public int direction; // Direction of the fireball, 1 for right, -1 for left
    public Sprite[] fireballSprites; // Assign 8 sprites in the Inspector
    public Sprite[] destroyedFireballSprites; // Assign 8 sprites for the destroyed fireball in the Inspector
    int damage = 3; // Damage dealt by the fireball
    ParticleSystem fireballParticleSystem; // Reference to the ParticleSystem component
    
    public float frameRate = 2f; // Time between frames
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private int currentIndex;
    private float timer;
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Entities";

        // Set the ParticleSystem sorting layer to "Entity" if it exists
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var psRenderer = ps.GetComponent<Renderer>();
            psRenderer.sortingLayerName = "Entities";
        }

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        direction = rb.velocity.x > 0 ? 1 : -1; // Set direction based on initial velocity
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            currentIndex = (currentIndex + 1) % fireballSprites.Length;
            spriteRenderer.sprite = fireballSprites[currentIndex];
            if (direction == -1)
            {
                spriteRenderer.flipX = true; // Flip the sprite for left direction
            }
            else
            {
                spriteRenderer.flipX = false; // Reset flip for right direction
            }
            timer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Fireball collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Fireball hit an enemy: " + collision.gameObject.name);
            EnemyScript enemyScript = collision.gameObject.GetComponent<EnemyScript>();
            if (enemyScript != null)
            {
                Debug.Log("Script isnt null, applying damage.");
                // Handle player collision with fireball
                enemyScript.health -= damage; // Assuming TakeDamage is a method in BasePlayerScript
                if (enemyScript.health > 0)
                {
                    enemyScript.animator.SetTrigger("Hurt"); // Trigger the hurt animation
                }
            }
        }

        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Fireball"))
        {
            Debug.Log("Fireball collided with: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag + " | Layer: " + LayerMask.LayerToName(collision.gameObject.layer));
            StartCoroutine(PlayDestroyedAnimation());
        }
    }

    private IEnumerator PlayDestroyedAnimation()
    {
        Debug.Log("Playing destroyed animation for fireball.");
        float destroyedFrameRate = 0.1f; // Adjust the frame rate for the destroyed animation
        int destroyedIndex = 0;

        while (destroyedIndex < destroyedFireballSprites.Length)
        {
            spriteRenderer.sprite = destroyedFireballSprites[destroyedIndex];
            if (direction == -1)
            {
                spriteRenderer.flipX = true; // Flip the sprite for left direction
            }
            else
            {
                spriteRenderer.flipX = false; // Reset flip for right direction
            }
            destroyedIndex++;
            yield return new WaitForSeconds(destroyedFrameRate);
        }

        Destroy(gameObject); // Destroy the fireball after the animation
    }
}