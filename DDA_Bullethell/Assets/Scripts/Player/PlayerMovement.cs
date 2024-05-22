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

    private float changeTime = 5f; // Time interval to change direction
    private float timer;
    private Vector2 movement;

    public float dashDistance = 5f;
    public float dashCooldown = 1f;
    public bool isDashing;
    private float dashCooldownTimer;
    public LayerMask obstacleLayerMask;

    [SerializeField] private GameObject afterimagePrefab;
    [SerializeField] private float afterimageLifetime = 0.5f;
    [SerializeField] private float afterimageSpawnInterval = 0.001f;
    private float afterimageSpawnTimer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (training)
        {
            timer = changeTime;
            ChooseNewDirection();
        }
    }

    void Update()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        if (isDashing)
        {
            afterimageSpawnTimer -= Time.deltaTime;
            if (afterimageSpawnTimer <= 0)
            {
                SpawnAfterimage();
                afterimageSpawnTimer = afterimageSpawnInterval;
            }
        }

    }

    void FixedUpdate()
    {
        if (this.dead || isDashing)
        {
            return;
        }
        if (training)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0)
            {
                ChooseNewDirection();
                timer = changeTime;
            }
        }

        // Apply movement
        rb.velocity = movement * speed;

        /// Rotate sprite towards movement direction
        if (movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }
    }

    public void Move(Vector2 move)
    {
        movement = move;
    }

    public void TryDash(Vector2 ifDirection)
    {
        if (dashCooldownTimer <= 0 && !isDashing)
        {
            Vector2 dashDirection = Vector2.zero;
            if (ifDirection == Vector2.zero)
            {
                dashDirection = movement.normalized; 
            }
            else
            {
                dashDirection = ifDirection.normalized;
            }
            StartCoroutine(Dash(dashDirection));
        }
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        afterimageSpawnTimer = 0;
        float elapsedTime = 0;
        float dashDuration = 0.1f; 

        Vector2 startPosition = rb.position;
        Vector2 endPosition = startPosition + direction * dashDistance;

        dashCooldownTimer = dashCooldown; // Start cooldown immediately upon dashing

        RaycastHit2D hit = Physics2D.Linecast(startPosition, endPosition, obstacleLayerMask);
        if (hit.collider != null)
        {
            // Adjust endPosition if there's an obstacle
            endPosition = hit.point - (direction * 0.1f);
        }

        while (elapsedTime < dashDuration)
        {
            rb.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
    private void SpawnAfterimage()
    {
        GameObject afterimage = Instantiate(afterimagePrefab, transform.position, transform.rotation);
        StartCoroutine(FadeAfterimage(afterimage));
    }

    IEnumerator FadeAfterimage(GameObject afterimage)
    {
        SpriteRenderer spriteRenderer = afterimage.GetComponent<SpriteRenderer>();
        Color initialColor = spriteRenderer.color;
        float elapsedTime = 0;

        while (elapsedTime < afterimageLifetime)
        {
            float alpha = Mathf.Lerp(initialColor.a, 0, elapsedTime / afterimageLifetime);
            if (spriteRenderer == null)
            {
                break;
            }
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (afterimage != null)
        {
            Destroy(afterimage);
        }
    }

    private void ChooseNewDirection()
    {
        if (UnityEngine.Random.value > 0.5f) // 50% chance
        {
            StartCoroutine(RestThenChangeDirection());
        }
        else
        {
            ChangeDirectionNow();
        }
    }

    private void ChangeDirectionNow()
    {
        float randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        movement = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        Debug.Log(movement);
    }

    IEnumerator RestThenChangeDirection()
    {
        // Set the movement to zero for resting
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        // Wait for 0.5 to 3 seconds
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2f));

        // Now change direction
        ChangeDirectionNow();
    }

    public Vector2 getMovement() { return movement; }
}
