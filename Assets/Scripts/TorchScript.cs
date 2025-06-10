using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchScript : MonoBehaviour
{
    public Sprite[] torchSprites; // Assign 8 sprites in the Inspector
    public float frameRate = 2f; // Time between frames

    private SpriteRenderer spriteRenderer;
    private int currentIndex;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentIndex = Random.Range(0, torchSprites.Length);
        spriteRenderer.sprite = torchSprites[currentIndex];
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            currentIndex = (currentIndex + 1) % torchSprites.Length;
            spriteRenderer.sprite = torchSprites[currentIndex];
            timer = 0f;
        }
    }
}