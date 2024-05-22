using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class IntermediateEnemyAI : Agent
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed;
    public float shootingInterval;
    public float moveSpeed;
    public float rotationSpeed;
    public bool training = false;


    private float shootingTimer;
    private Health healthComponent;

    private Rigidbody2D rb;

    private GameObject currentPlayerInstance;


    private bool HitPlayer = false;
    private bool KilledPlayer = false;
    private bool TookDamage = false;
    private bool CollidedWithObject = false;
    private bool Died = false;

    private float minX = -23, maxX = 23;
    private float minY = -12, maxY = 12;

    private Transform environmentParent;



    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent

        // Initialize variables or settings specific to the agent

        // Find and set the currentPlayerInstance to the player in the scene
        this.environmentParent = transform.parent;

        // Find the player within this local environment
        this.currentPlayerInstance = environmentParent.GetComponentInChildren<PlayerMovement>(true).gameObject;

        this.shootingTimer = shootingInterval;
        if (training)
        {
            this.currentPlayerInstance.GetComponent<Health>().OnPlayerDeath += () => KilledPlayer = true;
            this.healthComponent.OnTakeDamage += () => TookDamage = true;
            this.healthComponent.OnEnemyDeath += EnemyDied;
        }

        this.healthComponent = GetComponent<Health>();
        this.healthComponent.training = this.training;
        this.rb = GetComponent<Rigidbody2D>();
        this.GetComponent<PolygonCollider2D>().enabled = true;

    }

    private void EnemyDied(int difficultyValue)
    {
        Died = true;
    }
    public override void OnEpisodeBegin()
    {

        //Enemy
        this.KilledPlayer = false;
        this.GetComponent<PolygonCollider2D>().enabled = true;
        this.GetComponent<Health>().enabled = true;
        this.Died = false;
        this.healthComponent.currentHealth = this.healthComponent.maxHealth;

        if (training)
        {
            this.transform.localPosition = GetRandomStartPosition();
        }

        // Reset orientation
        this.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());
        if (training)
        {
            currentPlayerInstance.GetComponent<PlayerMovement>().dead = false;
            currentPlayerInstance.GetComponent<PlayerShooting>().dead = false;
            currentPlayerInstance.GetComponent<BoxCollider2D>().enabled = true;
        }

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //Own position
        sensor.AddObservation(transform.localPosition);
        // Relative Position to Player
        if (currentPlayerInstance != null)
        {
            Vector3 directionToPlayer = currentPlayerInstance.transform.localPosition - transform.localPosition;
            sensor.AddObservation(directionToPlayer.normalized); // Normalized direction
            sensor.AddObservation(directionToPlayer.magnitude); // Distance to player

            // Player's movement direction
            Vector2 playerVelocity = currentPlayerInstance.GetComponent<Rigidbody2D>().velocity;
            sensor.AddObservation(playerVelocity.normalized);
            sensor.AddObservation(playerVelocity.magnitude);
        }
        else
        {
            // If the player is not found, add zeros
            sensor.AddObservation(Vector3.zero); // Direction
            sensor.AddObservation(0f); // Distance
            sensor.AddObservation(Vector2.zero); // Player's movement direction normalized
            sensor.AddObservation(0f); // Player's movement magnitude
        }

        // Player's State
        if(currentPlayerInstance != null)
        {
            sensor.AddObservation(currentPlayerInstance.GetComponent<Health>().currentHealth / currentPlayerInstance.GetComponent<Health>().maxHealth); // Normalized health

        }
        else
        {
            sensor.AddObservation(0f); // Normalized health

        }

        // Enemy's Own State
        sensor.AddObservation(healthComponent.currentHealth / healthComponent.maxHealth); // Normalized health

        // Shooting Cooldown
        sensor.AddObservation(shootingTimer / shootingInterval); // Normalized shooting cooldown

        // Orientation or Rotation
        sensor.AddObservation(transform.rotation.eulerAngles / 360.0f); // Normalized rotation

        // Enemy's rotation relative to the player
        if (currentPlayerInstance != null)
        {
            Vector3 enemyForward = bulletSpawnPoint.up; // Using bulletSpawnPoint's up as forward direction
            Vector3 toPlayerDirection = (currentPlayerInstance.transform.position - transform.position).normalized;
            float relativeRotationAngle = Vector3.SignedAngle(enemyForward, toPlayerDirection, Vector3.forward) / 180.0f; // Normalize to range [-1, 1]
            sensor.AddObservation(relativeRotationAngle);
        }
        else
        {
            sensor.AddObservation(0f); // Default observation when player not found
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (training)
        {
            if (HitPlayer)
            {
                AddReward(1.0f); // Reward for hitting the player
            }
            if (KilledPlayer)
            {
                AddReward(4.0f); // Large reward for killing the player
                currentPlayerInstance.GetComponent<PlayerMovement>().dead = true;
                currentPlayerInstance.GetComponent<PlayerShooting>().dead = true;
                currentPlayerInstance.GetComponent<BoxCollider2D>().enabled = false;
                EndEpisode();
            }

            if (TookDamage)
            {
                AddReward(-0.5f); // Penalty for taking damage
            }

            if (CollidedWithObject)
            {
                AddReward(-0.5f); // Penalty for collision
            }

            if (Died)
            {
                AddReward(-2.0f); // Penalty for dying
                this.GetComponent<PolygonCollider2D>().enabled = false;
                this.GetComponent<Health>().enabled = false;
                EndEpisode();
            }
            if (this.KilledPlayer)
            {
                return;
            }
            if (!HitPlayer || !KilledPlayer)
            {
                AddReward(-0.02f * Time.fixedDeltaTime);
            }
        }
        if (Died)
        {
            return;
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
                StartCoroutine(ShootBurst()); // Start the burst shooting coroutine
                shootingTimer = shootingInterval; // Reset the shooting timer
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

    }

    public void Update()
    {
        shootingTimer -= Time.deltaTime;

        HitPlayer = false;
        TookDamage = false;
        CollidedWithObject = false;

    }


    private IEnumerator ShootBurst()
    {
        int shots = 3; // Number of shots in a burst
        float delayBetweenShots = 0.2f; // Delay between each shot in the burst

        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            for (int i = 0; i < shots; i++)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                Projectile projectileScript = bullet.GetComponent<Projectile>();
                projectileScript.targetTag = "Player"; // Set the target tag
                if (rb != null)
                {
                    rb.velocity = bulletSpawnPoint.up * bulletSpeed;
                    Destroy(bullet, 12.0f); // Adjust the time before destroying the projectile if needed
                }
                bullet.GetComponent<Projectile>().OnHitPlayer += () => HitPlayer = true;

                if (i < shots - 1) // Wait before shooting the next bullet, but not after the last bullet
                {
                    yield return new WaitForSeconds(delayBetweenShots);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        CollidedWithObject = true;
        




    }

    private void OnCollisionStay2D(Collision2D collision)
    {

       CollidedWithObject = true;



    }

    private Vector3 GetRandomStartPosition()
    {
        Vector3 startPosition = Vector3.zero;

        bool positionFound = false;

        while (!positionFound)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            float y = UnityEngine.Random.Range(minY, maxY);
            // Generate a random local position within a defined range
            startPosition = new Vector3(x, y, 0);

            // Check if the position collides with anything
            if (Physics2D.OverlapCircle(startPosition, 0.5f) == null)
            {
                positionFound = true;
            }
        }

        return startPosition;
    }

    private float GetRandomStartRotation()
    {
        // Generate a random rotation in degrees
        return UnityEngine.Random.Range(0f, 360f);
    }
}
