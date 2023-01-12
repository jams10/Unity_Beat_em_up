using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacter : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    SpriteRenderer spriteRenderer;
    float health;

    void Awake()
    {
        health = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(0, health - damage);
        spriteRenderer.flipY = !spriteRenderer.flipY;
    }
}
