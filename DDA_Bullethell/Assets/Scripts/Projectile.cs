using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 20;
    public string targetTag; // Tag of the target it can damage

    public event Action OnHitPlayer;

    public TrailRenderer bulletTrail;

    void Start()
    {
        AddTrailRenderer();
    }

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
        if (hitInfo.gameObject.CompareTag("Wall") || hitInfo.gameObject.CompareTag("Hazard"))
        {
            Destroy(gameObject);
        }

    }

    private void AddTrailRenderer()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.time = 0.5f; // Duration of trail
        trail.startWidth = 0.1f;
        trail.endWidth = 0.0f;

        // Set the color of the trail based on who shoots the projectile
        if (targetTag == "Player")
        {
            // Enemy shoots, so make the trail red
            trail.startColor = Color.red;
            trail.endColor = new Color(1, 0, 0, 0); // Fade to transparent
        }
        else if (targetTag == "Enemy")
        {
            // Player shoots, so make the trail blue
            trail.startColor = Color.blue;
            trail.endColor = new Color(0, 0, 1, 0); // Fade to transparent
        }
        this.bulletTrail = trail;
    }
}
