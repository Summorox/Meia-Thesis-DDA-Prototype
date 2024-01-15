using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 20;
    public string targetTag; // Tag of the target it can damage

    public event Action OnHitPlayer;
    public event Action OnKillPlayer;

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Check if the collided object has the correct tag
        if (hitInfo.gameObject.CompareTag(targetTag))
        {
            Debug.Log("Hit: " + hitInfo.gameObject.name);
            if (hitInfo.gameObject.CompareTag("Player"))
            {
                PlayerParrying parrying = hitInfo.gameObject.GetComponent<PlayerParrying>();

                if (parrying == null || !parrying.isParrying)
                {
                    Health health = hitInfo.GetComponent<Health>();
                    if (health != null)
                    {
                        OnHitPlayer?.Invoke();
                        health.TakeDamage(damage);
                        if(health.currentHealth <= 0) { 
                
                            OnKillPlayer?.Invoke();
                        }
                    }
                    Destroy(gameObject); // Destroy the projectile on hit.
                }

            }
            else 
            {
                Health health = hitInfo.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                Destroy(gameObject); // Destroy the projectile on hit.
            }



        }
        if (hitInfo.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }

    }
}
