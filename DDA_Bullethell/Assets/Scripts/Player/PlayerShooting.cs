using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate;
    public float projectileSpeed;
    public bool training = false;
    public bool dead = false;

    private float nextFireTime = 0.0f;
    private PerformanceMetricsLogger metricsLogger;

    void Start()
    {
        metricsLogger = GetComponent<PerformanceMetricsLogger>();

    }
    void Update()
    {
        if (this.dead)
        {
            return;
        }
        if (training)
        {
            if (Time.time > nextFireTime)
            {
                nextFireTime = Time.time + 1 / fireRate;
                GameObject nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    ShootTowards(nearestEnemy.transform.position);
                }
            }
        }
      
    }

    public void Shoot(Vector2 targetPosition)
    {
        if (Time.time > nextFireTime) 
        {
            nextFireTime = Time.time + 1 / fireRate;
            ShootTowards(targetPosition);
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float minDistance = Mathf.Infinity;
        Vector2 position = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(enemy.transform.position, position);
            if (distance < minDistance)
            {
                nearestEnemy = enemy;
                minDistance = distance;
            }
        }

        return nearestEnemy;
    }

    private void ShootTowards(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.targetTag = "Enemy"; // Set the target tag

        SpriteRenderer projectileSprite = projectile.GetComponent<SpriteRenderer>();
        if(metricsLogger != null)
        {
            metricsLogger.LogShotFired();
            projectile.GetComponent<Projectile>().OnHitEnemy += metricsLogger.LogShotHit;
        }
        if (projectileSprite != null)
        {
            projectileSprite.color = Color.blue;
        }

        Destroy(projectile, 12.0f); // Destroy the projectile after 12 seconds
    }
}
