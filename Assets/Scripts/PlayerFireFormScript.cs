using UnityEngine;
using System.Collections;

public class PlayerFireFormScript : MonoBehaviour
{
    public GameObject player;
    public PlayerMovementScript playerMovementScript; // Reference to the PlayerMovementScript
    public int transformationDuration = 5; // Duration in seconds for the transformation
    public bool canWalkOnFire;

    public GameObject fireballPrefab; // Prefab for the fireball
    public int fireballCounter;
    public int maxFireballs = 3; // Maximum number of fireballs that can be fired at once

    public int initialCastSpeed = 20;
    public int fireballCooldown = 3; // Cooldown time in seconds for firing fireballs
    public int fireballBaseTimeToLive = 5; // Timer to track fireball cooldown
    public bool canFireFireball; // Flag to check if fireball can be fired
    public GameObject fireballHand;

    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        fireballHand.SetActive(true); 
        playerMovementScript = player.GetComponent<PlayerMovementScript>();
        canWalkOnFire = true;
        canFireFireball = true;
        fireballCounter = 0;
    }

    void OnDisable()
    {
        fireballHand.SetActive(false);
        canWalkOnFire = false;
        canFireFireball = false;
        fireballCounter = 0;
    }

    void Update()
    {
        if (playerMovementScript.direction == -1)
        {
            fireballHand.transform.localPosition = new Vector2(-0.5f, fireballHand.transform.localPosition.y);
        }
        else if (playerMovementScript.direction == 1)
        {
            fireballHand.transform.localPosition = new Vector2(0.5f, fireballHand.transform.localPosition.y);
        }

        // Check if the player can fire a fireball
        if (canFireFireball && Input.GetKeyDown(KeyCode.E))
        {
            FireFireball();
        }
    }

    public void FireFireball()
    {
        if (fireballCounter < maxFireballs) // Limit to 3 fireballs
        {
            GameObject fireball = Instantiate(fireballPrefab, fireballHand.transform.position, Quaternion.identity);
            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            fireballRb.velocity = new Vector2(playerMovementScript.direction * initialCastSpeed, 0f);
            fireballCounter++;
            StartCoroutine(FireballCooldown());
            StartCoroutine(FireballLifetime(fireball));
        }
    }

    public IEnumerator FireballCooldown()
    {
        canFireFireball = false;
        yield return new WaitForSeconds(fireballCooldown);
        canFireFireball = true;
    }

    public IEnumerator FireballLifetime(GameObject fireball)
    {
        yield return new WaitForSeconds(fireballBaseTimeToLive);
        Destroy(fireball);
        fireballCounter--;
    }
}