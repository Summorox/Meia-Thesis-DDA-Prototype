using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Create movement vector
        Vector2 movement = new Vector2(horizontalInput, verticalInput);

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
}
