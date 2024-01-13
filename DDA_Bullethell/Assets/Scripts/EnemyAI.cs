using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 5f;
    public float shootingInterval = 2f;
    public float moveSpeed = 2f;
    private Transform playerTransform;
    private float shootingTimer;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        shootingTimer = shootingInterval;
    }

    void Update()
    {
        MoveTowardsPlayer();
        HandleShooting();
    }

    void MoveTowardsPlayer()
    {
        if (playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // Move the enemy towards the player
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            // Rotate the enemy to face the player smoothly
            RotateTowardsPlayer(direction);
        }
    }

    void RotateTowardsPlayer(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90; // Adjusting by -90 degrees to align with the capsule's up direction
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }

    void HandleShooting()
    {
        if (shootingTimer <= 0f)
        {
            Shoot();
            shootingTimer = shootingInterval;
        }
        else
        {
            shootingTimer -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            projectileScript.targetTag = "Player"; // Set the target tag
            if (rb != null)
            {
                rb.velocity = bulletSpawnPoint.right * bulletSpeed;

                Destroy(bullet, 4.0f); // Destroy the projectile after 4 seconds
            }
        }
    }
}
