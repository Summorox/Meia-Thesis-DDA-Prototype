using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody2D rb;
    public bool training = false;
    public bool dead = false;

    private float changeTime = 1.5f; // Time interval to change direction
    private float timer;
    private Vector2 movement;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = changeTime;
        ChooseNewDirection();
    }

    void FixedUpdate()
    {
        if (this.dead)
        {
            return;
        }
        if (training)
        {
            timer -= Time.fixedDeltaTime;
            Debug.Log("Test");
            if (timer <= 0)
            {
                ChooseNewDirection();
                timer = changeTime;
            }
        }
        else
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            // Create movement vector
            movement = new Vector2(horizontalInput, verticalInput);
        }
       
        // Optional: Adjust diagonal movement speed
        // if (movement.magnitude > 1)
        //     movement.Normalize(); // or use custom scaling

        // Apply movement
        rb.velocity = movement * speed;

        /// Rotate sprite towards movement direction
        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }
    }

    private void ChooseNewDirection()
    {
        float randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        movement = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }
}
