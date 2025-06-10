using UnityEngine;
using System.Collections;

public class PlayerFireFormScript : MonoBehaviour
{
    public GameObject player;
    public Animator animator; // Reference to the Animator component
    public BasePlayerScript playerScript; // Reference to the PlayerMovementScript
    public bool canWalkOnFire;

    public GameObject fireballPrefab; // Prefab for the fireball
    public int fireballCounter;
    public int maxFireballs = 3; // Maximum number of fireballs that can be fired at once
    public int fireballDamage = 3; // Damage dealt by the fireball

    private int initialCastSpeed = 15;
    public int baseFireballCooldown = 3; // Cooldown time in seconds for firing fireballs
    public float fireballCooldown = 3f; // Cooldown time in seconds for firing fireballs
    [SerializeField] private float fireballBaseTimeToLive = 0.5f; // Timer to track fireball cooldown
    public bool canFireFireball; // Flag to check if fireball can be fired
    public GameObject fireballHand;

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        fireballHand.SetActive(true); 
        playerScript = player.GetComponent<BasePlayerScript>();
        canWalkOnFire = true;
        canFireFireball = true;
        fireballCounter = GameObject.FindGameObjectsWithTag("Fireball").Length; // Count existing fireballs
    }

    void OnDisable()
    {
        fireballHand.SetActive(false);
        canWalkOnFire = false;
        canFireFireball = false;
    }

    void Update()
    {
        if (playerScript.direction == -1)
        {
            fireballHand.transform.localPosition = new Vector2(-0.5f, fireballHand.transform.localPosition.y);
        }
        else if (playerScript.direction == 1)
        {
            fireballHand.transform.localPosition = new Vector2(0.5f, fireballHand.transform.localPosition.y);
        }

        // Check if the player can fire a fireball
        if (canFireFireball && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FireFireball());
        }
    }

    public IEnumerator FireFireball()
    {
        canFireFireball = false; // Prevent firing while the animation is playing
        if (fireballCounter < maxFireballs) // Limit to 3 fireballs
        {
            animator.SetBool("Attacking", true); // Set the attacking animation state
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("FireFormAttack"))
            {
                yield return null;
            }

            // Wait until the animation reaches the 11th frame
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float targetNormalizedTime = 22f / stateInfo.length / 60f; // 11th frame at 60 FPS

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < (22f / stateInfo.length / 60f))
            {
                yield return null;
            }

            // Instantiate fireball at the 11th frame
            GameObject fireball = Instantiate(fireballPrefab, fireballHand.transform.position, Quaternion.identity);
            Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
            fireballRb.velocity = new Vector2(playerScript.direction * initialCastSpeed, 0f);
            fireballCounter++;

            // Wait until the animation finishes
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("FireFormAttack") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }
            canFireFireball = true; // Allow firing again after the animation finishes
            animator.SetBool("Attacking", false);
            StartCoroutine(FireballCooldown());
            StartCoroutine(FireballLifetime(fireball));
        }
    }

    public IEnumerator FireballCooldown()
    {
        canFireFireball = false;
        while (fireballCooldown > 0)
        {
            yield return new WaitForSeconds(1f);
            fireballCooldown--;
        }
        canFireFireball = true;
        fireballCooldown = baseFireballCooldown; // Reset cooldown
    }

    public IEnumerator FireballLifetime(GameObject fireball)
    {
        yield return new WaitForSeconds(fireballBaseTimeToLive);
        Destroy(fireball);
        fireballCounter--;
    }
}