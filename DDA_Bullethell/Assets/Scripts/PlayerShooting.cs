using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireRate;
    public float projectileSpeed;

    private float nextFireTime = 0.0f;


    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > nextFireTime) // 0 for left click
        {
            nextFireTime = Time.time + 1 / fireRate;
            ShootProjectile();
        }
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
