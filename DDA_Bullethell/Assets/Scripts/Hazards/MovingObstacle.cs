using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public float moveDistance = 5.0f; // Distance the obstacle will move from its original position
    public float speed = 2.0f;
    public Vector2 moveDirection = Vector2.up; // Direction of movement 

    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 target;

    void Start()
    {
        // Calculate start and end points based on the moveDistance and moveDirection
        startPoint = transform.position;
        endPoint = startPoint + (Vector3)(moveDirection.normalized * moveDistance);
        target = endPoint;
    }

    void Update()
    {
        MoveObstacle();
    }

    void MoveObstacle()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Switch the target when the obstacle reaches one of the points
        if (Vector3.Distance(transform.position, target) < 0.001f)
        {
            target = target == startPoint ? endPoint : startPoint;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // If colliding with a wall, switch direction immediately
            target = target == startPoint ? endPoint : startPoint;
        }
    }
}
