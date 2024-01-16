using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate;
    public float projectileSpeed;
    public bool training = false;
    public bool dead = false;

    private float nextFireTime = 0.0f;


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
        else
        {
            if (Input.GetMouseButtonDown(0) && Time.time > nextFireTime) // 0 for left click
            {
                nextFireTime = Time.time + 1 / fireRate;
                ShootProjectile();
            }
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

        Destroy(projectile, 4.0f); // Destroy the projectile after 4 seconds
    }

    private void ShootProjectile()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * projectileSpeed;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.targetTag = "Enemy"; // Set the target tag

        Destroy(projectile, 4.0f); // Destroy the projectile after 4 seconds
    }
}
