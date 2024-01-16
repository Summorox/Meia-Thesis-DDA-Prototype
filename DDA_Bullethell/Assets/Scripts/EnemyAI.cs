using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyAI : Agent
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed;
    public float shootingInterval;
    public float moveSpeed;
    public float rotationSpeed;
    public bool training=false;
    public GameObject currentPlayerInstance; // Reference to the player prefab


    private float shootingTimer;
    private Health healthComponent;
  
    private Rigidbody2D rb;


    private bool HitPlayer = false;
    private bool KilledPlayer = false;
    private bool TookDamage = false;
    private bool CollidedWithObject = false;
    private bool Died = false;



    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent

        // Initialize variables or settings specific to the agent
        shootingTimer = shootingInterval;
        currentPlayerInstance.GetComponent<PlayerMovement>().training = this.training;
        currentPlayerInstance.GetComponent<PlayerShooting>().training = this.training;
        currentPlayerInstance.GetComponent<Health>().training = this.training;
        currentPlayerInstance.GetComponent<Health>().OnDeath += () => KilledPlayer = true;

        healthComponent = GetComponent<Health>();
        healthComponent.training = this.training;
        rb = GetComponent<Rigidbody2D>();
        healthComponent.OnTakeDamage += () => TookDamage = true;
        healthComponent.OnDeath += () => Died = true;

    }

    public override void OnEpisodeBegin()
    {

        //Player
        // Instantiate a new player instance
        currentPlayerInstance.transform.localPosition = GetRandomStartPosition();

        // Reset orientation
        currentPlayerInstance.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());

        currentPlayerInstance.GetComponent<PlayerMovement>().dead = false;
        currentPlayerInstance.GetComponent<PlayerShooting>().dead = false;
        currentPlayerInstance.GetComponent<BoxCollider2D>().enabled = true;

        this.KilledPlayer = false;


        //Enemy
        // Reset the position of the enemy agent
        this.transform.localPosition = GetRandomStartPosition();

        // Reset orientation
        this.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());

        this.GetComponent<PolygonCollider2D>().enabled = true;
        this.Died = false;

        healthComponent.currentHealth = healthComponent.maxHealth;

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //Own position
        sensor.AddObservation(transform.localPosition);
        // Relative Position to Player
        if (currentPlayerInstance != null)
        {
            Vector3 directionToPlayer = currentPlayerInstance.GetComponent<Transform>().localPosition - this.transform.localPosition;
            sensor.AddObservation(directionToPlayer.normalized); // Normalized direction
            sensor.AddObservation(directionToPlayer.magnitude); // Distance to player
        }
        else
        {
            // If the player is not found, add zeros
            sensor.AddObservation(Vector3.zero); // Direction
            sensor.AddObservation(0f); // Distance
        }

        // Enemy's Own State
        sensor.AddObservation(healthComponent.currentHealth / healthComponent.maxHealth); // Normalized health

        // Shooting Cooldown
        sensor.AddObservation(shootingTimer / shootingInterval); // Normalized shooting cooldown

        // Orientation or Rotation
        sensor.AddObservation(transform.rotation.eulerAngles / 360.0f); // Normalized rotation
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (training)
        {
            if (HitPlayer)
            {
                Debug.Log("Hit Player");
                AddReward(0.3f); // Reward for hitting the player
            }

            if (KilledPlayer)
            {
                AddReward(1.0f); // Large reward for killing the player
                Debug.Log("Killed Player 2");
                currentPlayerInstance.GetComponent<PlayerMovement>().dead = true;
                currentPlayerInstance.GetComponent<PlayerShooting>().dead = true;
                currentPlayerInstance.GetComponent<BoxCollider2D>().enabled = false;
                EndEpisode();
            }

            if (TookDamage)
            {
                AddReward(-0.2f); // Penalty for taking damage
            }

            if (CollidedWithObject)
            {
                Debug.Log("Collided");
                AddReward(-0.1f); // Penalty for collision
            }

            if (Died)
            {
                AddReward(-0.5f); // Penalty for dying
                this.GetComponent<PolygonCollider2D>().enabled = false;
                EndEpisode();
            }
            if (this.Died)
            {
                return;
            }
        }
        // Movement
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        Vector2 direction = new Vector2(moveX, moveY).normalized;

        Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Rotation
        float rotationChange = actions.ContinuousActions[2] * rotationSpeed * Time.deltaTime;
        float newRotation = rb.rotation + rotationChange;
        rb.MoveRotation(newRotation);
      

        // Shooting
        bool shoot = actions.DiscreteActions[0] == 1;
        if (shoot)
        {
            if (shootingTimer <= 0f)
            {
                Shoot();
                shootingTimer = shootingInterval;
            }
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
    
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Movement: Map arrow keys or WASD to movement actions
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Left/Right or A/D
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // Up/Down or W/S

        // Rotation: Map QE keys to rotation
        continuousActionsOut[2] = (Input.GetKey(KeyCode.Q) ? -1f : 0f) + (Input.GetKey(KeyCode.E) ? 1f : 0f);

        // Shooting: Map space key to shooting
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        Debug.Log(continuousActionsOut[2]);
        Debug.Log(discreteActionsOut[0]);
    }
    
    public void Update()
    {
        shootingTimer -= Time.deltaTime;

        HitPlayer = false;
        TookDamage = false;
        CollidedWithObject = false;

        //AddReward(-0.05f * Time.deltaTime);

    }

    private Vector3 GetRandomStartPosition()
    {
        Vector3 startPosition = Vector3.zero;

        bool positionFound = false;

        while (!positionFound)
        {
            // Generate a random local position within a defined range
            startPosition = new Vector3(Random.Range(-10, 10), Random.Range(-4, 6), 0);

            // Check if the position collides with anything
            if (Physics2D.OverlapCircle(startPosition, 0.1f) == null)
            {
                positionFound = true;
            }
        }

        return startPosition;
    }

    private float GetRandomStartRotation()
    {
        // Generate a random rotation in degrees
        return Random.Range(0f, 360f);
    }


    private void Shoot()
    {
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            projectileScript.targetTag = "Player"; // Set the target tag
            if (rb != null)
            {
                rb.velocity = bulletSpawnPoint.up * bulletSpeed;

                Destroy(bullet, 2.0f); // Destroy the projectile after 2 seconds
            }
            bullet.GetComponent<Projectile>().OnHitPlayer += () => HitPlayer = true;
            AddReward(-0.01f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        CollidedWithObject = true;

    }
}
