using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBallMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 2f;
    public int damageAmount = 50;
    private Vector2 movementDirection;
    private float changeDirectionTimer;

    void Start()
    {
        ChangeDirection();
    }

    void Update()
    {
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0)
        {
            ChangeDirection();
        }
    }

    void FixedUpdate()
    {
        // Move the spike ball in the chosen direction
        GetComponent<Rigidbody2D>().velocity = movementDirection * moveSpeed;
    }

    void ChangeDirection()
    {
        // Randomly choose a new direction
        movementDirection = Random.insideUnitCircle.normalized;
        changeDirectionTimer = changeDirectionInterval;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hazard"))
        {
            ChangeDirection();
            return; // Skip the rest of the method if it's a wall collision
        }
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            // Push back and hurt the player or enemy
            Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(pushDirection * 500); // Adjust force as needed

            collision.gameObject.GetComponent<Health>().TakeDamage(damageAmount);
        }
    }

}