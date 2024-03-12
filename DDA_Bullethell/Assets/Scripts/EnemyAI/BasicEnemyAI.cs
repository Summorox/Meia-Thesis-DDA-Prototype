using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicEnemyAI : Agent
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed;
    public float shootingInterval;
    public float moveSpeed;
    public float rotationSpeed;
    public bool training=false;


    private float shootingTimer;
    private Health healthComponent;
  
    private Rigidbody2D rb;

    private GameObject currentPlayerInstance;


    private bool HitPlayer = false;
    private bool KilledPlayer = false;
    private bool TookDamage = false;
    private bool CollidedWithObject = false;
    private bool Died = false;



    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent

        // Initialize variables or settings specific to the agent

        // Find and set the currentPlayerInstance to the player in the scene
        var environmentParent = transform.parent; 

        // Find the player within this local environment
        currentPlayerInstance = environmentParent.GetComponentInChildren<PlayerMovement>(true).gameObject;

        shootingTimer = shootingInterval;
        currentPlayerInstance.GetComponent<Health>().OnDeath += () => KilledPlayer = true;

        healthComponent = GetComponent<Health>();
        healthComponent.training = this.training;
        rb = GetComponent<Rigidbody2D>();
        healthComponent.OnTakeDamage += () => TookDamage = true;
        healthComponent.OnDeath += () => Died = true;

    }

    public override void OnEpisodeBegin()
    {

        //Enemy
        this.KilledPlayer = false;
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
        //Player's movement direction
        Vector2 playerVelocity = currentPlayerInstance.GetComponent<Rigidbody2D>().velocity;
        sensor.AddObservation(playerVelocity.normalized);
        sensor.AddObservation(playerVelocity.magnitude);

        // Player's State
        sensor.AddObservation(currentPlayerInstance.GetComponent<Health>().currentHealth / currentPlayerInstance.GetComponent<Health>().maxHealth); // Normalized health

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
                AddReward(0.5f); // Reward for hitting the player
            }

            if (KilledPlayer)
            {
                Debug.Log("Killed Player");
                AddReward(4.0f); // Large reward for killing the player
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
                AddReward(-0.1f); // Penalty for collision
            }

            if (Died)
            {
                AddReward(-0.5f); // Penalty for dying
                this.GetComponent<PolygonCollider2D>().enabled = false;
                EndEpisode();
            }
            if (this.Died || this.KilledPlayer)
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
        if(!HitPlayer || !KilledPlayer)
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
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

    }
    
    public void Update()
    {
        shootingTimer -= Time.deltaTime;

        HitPlayer = false;
        TookDamage = false;
        CollidedWithObject = false;

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

                Destroy(bullet, 12.0f); // Destroy the projectile after 2 seconds
            }
            bullet.GetComponent<Projectile>().OnHitPlayer += () => HitPlayer = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hazard"))
        {
            CollidedWithObject = true;
        }

 


    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hazard"))
        {
            // Apply a continuous penalty for staying in collision
            CollidedWithObject = true;
        }


    }
}
