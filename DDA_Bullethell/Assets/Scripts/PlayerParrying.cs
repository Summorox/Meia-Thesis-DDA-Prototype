using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParrying : MonoBehaviour
{
    public float parryDuration; // Duration of parry window in seconds
    public float parryCooldown; // Cooldown duration in seconds
    public float deflectedProjectileSpeed; // Speed of deflected projectile

    public bool isParrying = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;

    public ParticleSystem parryEffect;

    void Update()
    {
        HandleParryInput();
        UpdateParryState();
    }

    void HandleParryInput()
    {
        if (cooldownTimer > 0)
        {
            // Reduce the cooldown timer
            cooldownTimer -= Time.deltaTime;
        }
        else if (Input.GetMouseButtonDown(1) && !isParrying) // Right mouse button
        {
            StartParry();
        }
    }

    void StartParry()
    {
        isParrying = true;
        parryTimer = parryDuration;
        cooldownTimer = parryCooldown;

        if (parryEffect != null)
        {
            parryEffect.Play(); // Play the parry effect
        }
    }

    void UpdateParryState()
    {
        if (isParrying)
        {
            if (parryTimer > 0)
            {
                // Reduce the parry timer
                parryTimer -= Time.deltaTime;
            }
            else
            {
                // End parrying
                EndParry();
            }
        }
    }

    void EndParry()
    {
        isParrying = false;
        if (parryEffect != null && parryEffect.isPlaying)
        {
            parryEffect.Stop(); // Stop the parry effect
        }
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (isParrying && collider.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("Collision Detected.");
            Projectile projectile = collider.gameObject.GetComponent<Projectile>();
            if(projectile.targetTag == "Player")
            {
                // Parry successful, deflect the projectile
                DeflectProjectile(collider.gameObject);
            }

       
        }
    }

    void DeflectProjectile(GameObject projectile)
    {
        // Determine direction from player to projectile
        Vector2 directionToProjectile = (projectile.transform.position - transform.position).normalized;

        // Apply the new velocity to the projectile
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = directionToProjectile * deflectedProjectileSpeed;
        }

        // Change the projectile's tag or layer so it can damage enemies
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.targetTag = "Enemy";

        // Add additional effects like sound or visual feedback here
    }
}
