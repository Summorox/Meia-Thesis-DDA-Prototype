using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParrying : MonoBehaviour
{
    public float parryDuration; // Duration of parry window in seconds
    public float parryCooldown; // Cooldown duration in seconds
    public float deflectedProjectileSpeed; // Speed of deflected projectile
    public ParticleSystem deflectionIndicator;



    public bool isParrying = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;
    private bool parrySuccess = false;

    public ParticleSystem parryEffect;

    private PerformanceMetricsLogger metricsLogger;

    public bool managerTraining = true;

    void Start()
    {
        metricsLogger = GetComponent<PerformanceMetricsLogger>();

    }


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
    }

    public void Parry()
    {
        if (!isParrying)
        {
            metricsLogger.LogParryAttempt();
            StartParry();
        }
    }

    void StartParry()
    {
        isParrying = true;
        parryTimer = parryDuration;
        cooldownTimer = parryCooldown;

        if (parryEffect != null && !managerTraining)
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
        if (parrySuccess)
        {
            metricsLogger.LogParrySuccess();
            parrySuccess = false;
        }
        if (parryEffect != null && parryEffect.isPlaying)
        {
            parryEffect.Stop(); // Stop the parry effect
        }
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (isParrying && collider.gameObject.CompareTag("Projectile"))
        {
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
        parrySuccess = true;
        // Determine the direction from the player to the projectile
        Vector2 directionToProjectile = (projectile.transform.position - transform.position).normalized;

<<<<<<< Updated upstream
        if (!managerTraining){
            // Calculate the X rotation so that the cone is emitted towards the deflected projectile
            float angleX = Mathf.Atan2(directionToProjectile.y, directionToProjectile.x) * Mathf.Rad2Deg;

            // Since we want the particle system to emit in the direction of the deflected projectile,
            Quaternion rotation = Quaternion.Euler(angleX, 90, -90);

            // Instantiate the particle system with the calculated orientation
            ParticleSystem deflectEffect = Instantiate(deflectionIndicator, projectile.transform.position, rotation);

            if (deflectEffect != null)
            {
                deflectEffect.Play();
            }
        }
=======
        if (!managerTraining)
        {
            // Calculate the X rotation so that the cone is emitted towards the deflected projectile
            float angleX = Mathf.Atan2(directionToProjectile.y, directionToProjectile.x) * Mathf.Rad2Deg;
>>>>>>> Stashed changes

            // Since we want the particle system to emit in the direction of the deflected projectile,
            Quaternion rotation = Quaternion.Euler(angleX, 90, -90);

            // Instantiate the particle system with the calculated orientation
            ParticleSystem deflectEffect = Instantiate(deflectionIndicator, projectile.transform.position, rotation);

            if (deflectEffect != null)
            {
                deflectEffect.Play();
            }
        }

        // Apply the new velocity to the projectile
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = directionToProjectile * deflectedProjectileSpeed;
        }

        // Change the projectile's tag or layer so it can damage enemies
        Projectile proj = projectile.GetComponent<Projectile>();
        proj.targetTag = "Enemy";

        SpriteRenderer projectileSprite = projectile.GetComponent<SpriteRenderer>();
        if (projectileSprite != null)
        {
            projectileSprite.color = Color.blue;
        }
        TrailRenderer bulletTrail = projectile.GetComponent<Projectile>().bulletTrail;
        if(bulletTrail != null)
        {
            bulletTrail.startColor = Color.blue;
            bulletTrail.endColor = new Color(0, 0, 1, 0); // Fade to transparent
        }

        // Add additional effects like sound or visual feedback here
    }
}
