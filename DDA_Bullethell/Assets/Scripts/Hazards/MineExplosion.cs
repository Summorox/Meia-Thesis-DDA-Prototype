using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MineExplosion : MonoBehaviour
{
    public ParticleSystem explosionEffect;
    public float explosionRadius = 5f;
    public float explosionForce = 500f;
    public int damage = 50;

    private void Start()
    {
        GetComponent<PolygonCollider2D>().enabled = true;
        this.GetComponent<Health>().OnDeath += () => detonate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            detonate();
            
        }
    }

    private void detonate()
    {
        // Trigger explosion effect
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        // Find all colliders within the explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if(hit.GetComponent<Health>() != null)
                {
                    // Apply force away from the explosion center
                    Vector2 forceDirection = rb.position - (Vector2)transform.position;
                    rb.AddForce(forceDirection.normalized * explosionForce);

                    hit.GetComponent<Health>().TakeDamage(damage);
                }
            }
        }

        // Destroy the mine object
        Destroy(gameObject);
        
    }

    
}
