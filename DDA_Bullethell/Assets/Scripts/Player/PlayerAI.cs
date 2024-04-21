using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public enum DifficultyLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}
public class PlayerAI : Agent
{
    private Vector2 movementInput; // To store movement input
    private Rigidbody2D rb; // Rigidbody 
    private bool dashRequested = false;
    private bool parryRequested = false;
    private PlayerMovement playerMovement;
    private PlayerParrying playerParrying;
    private PlayerShooting playerShooting;
    private Health healthComponent;
    public bool training;

    public DifficultyLevel gameDifficulty;

    public bool Died = false;
    private bool useHeuristics = false;
    private bool shootingRequested;

    private PerformanceMetricsLogger metrics;
    private LevelManager levelManager;

    public override void Initialize()
    {
        metrics = GetComponent<PerformanceMetricsLogger>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerParrying = GetComponent<PlayerParrying>();
        playerShooting = GetComponent<PlayerShooting>();
        healthComponent = GetComponent<Health>();
        if (training)
        {
            healthComponent.OnTakeDamage += TookDamage;
            metrics.HitTarget += HitTarget;
            metrics.Parried += Parried;
            metrics.killedEnemy += KilledEnemy;
            levelManager = GetComponentInParent<LevelManager>(); // Find the LevelManager instance
            if (levelManager != null)
            {
                levelManager.OnWaveFinished += EvaluateWaveMetrics; // Subscribe to the event
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        this.Died = false;
        metrics.ResetMetrics();
        Debug.Log("Player episode Begin");

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Local Position
        sensor.AddObservation(transform.localPosition);

        //Current Health
        sensor.AddObservation(healthComponent.currentHealth / healthComponent.maxHealth);

        //Current Accuracy
        sensor.AddObservation(metrics.getAccuracy());

        //Current Parry Success Rate
        sensor.AddObservation(metrics.getParrySuccessRate());

        //Current Wave Reached
        if (ValidateObservation(metrics.getCurrentWave()))
        {
            sensor.AddObservation(metrics.getCurrentWave());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Current Kill Score
        if (ValidateObservation(metrics.getKillScore()))
        {
            sensor.AddObservation(metrics.getKillScore());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Current Average Health Lost Per Wave
        if (ValidateObservation(metrics.getAverageHealthLostPerWave()))
        {
            sensor.AddObservation(metrics.getAverageHealthLostPerWave());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Current Average Time spent per Wave
        if (ValidateObservation(metrics.getAverageWaveCompletionTime()))
        {
            sensor.AddObservation(metrics.getAverageWaveCompletionTime());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Velocity
        Vector2 playerVelocity = GetComponent<Rigidbody2D>().velocity;
        sensor.AddObservation(playerVelocity.normalized);
        sensor.AddObservation(playerVelocity.magnitude);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            float closestDistance = enemies.Min(enemy => Vector2.Distance(transform.position, enemy.transform.position));
            //Closest distance to enemy
            sensor.AddObservation(closestDistance);

            //Enemy density within a radius
            sensor.AddObservation(enemies.Count(enemy => Vector2.Distance(transform.position, enemy.transform.position) < 8f));
        }
        else
        {
            //Closest distance to enemy
            sensor.AddObservation(0f);

            //Enemy density within a radius
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (training)
        {
            if (this.Died)
            {
                return;
            }
            AdjustRewards();
        }
        // Movement
        movementInput.x = actions.ContinuousActions[0];
        movementInput.y = actions.ContinuousActions[1];
        playerMovement.Move(movementInput);

        //Dash
        if (actions.DiscreteActions[0] == 1)
        {
            Debug.Log("Wants to Dash");
            bool dash = true;
            Vector2 ifDirection = Vector2.zero;
            if (!useHeuristics)
            {
                float randomChance = UnityEngine.Random.value;
                switch (gameDifficulty)
                {
                    case DifficultyLevel.Low:
                        break;
                    case DifficultyLevel.Medium:
                        if(randomChance > 0.50f)
                        {
                            dash = IsDashSafe(out ifDirection);
                        }
                        break;
                    case DifficultyLevel.High:
                        if (randomChance > 0.25f)
                        {
                            dash = IsDashSafe(out ifDirection);
                        }
                        break;
                    case DifficultyLevel.VeryHigh:
                        dash = IsDashSafe(out ifDirection);
                        break;
                }
            }
            if(dash) {
                playerMovement.TryDash(ifDirection);
            }
            dashRequested = false;
        }

        //Parrying
        if (actions.DiscreteActions[1] == 1)
        {
            bool parry = true;
            if (!useHeuristics)
            {
                switch (gameDifficulty)
                {
                    case DifficultyLevel.Low:
                        break;
                    case DifficultyLevel.Medium:
                        parry = IsProjectileInRange(8f);
                        break;
                    case DifficultyLevel.High:
                        parry = IsProjectileInRange(5f);
                        break;
                    case DifficultyLevel.VeryHigh:
                        parry = IsProjectileInRange(3f);
                        break;
                }
            }
            if (parry)
            {
                playerParrying.Parry();
            }
            parryRequested = false;
        }

        //Shooting
        Vector2 shootDirection = new Vector2(actions.ContinuousActions[2], actions.ContinuousActions[3]).normalized;  

        if (actions.DiscreteActions[2] == 1)
        {
            if (!useHeuristics)
            {
                switch (gameDifficulty)
                {
                    case DifficultyLevel.Low:
                        break;
                    case DifficultyLevel.Medium:
                        shootDirection = AdjustShootDirection(shootDirection, 6f);
                        break;
                    case DifficultyLevel.High:
                        shootDirection = AdjustShootDirection(shootDirection, 4f);
                        break;
                    case DifficultyLevel.VeryHigh:
                        shootDirection = AdjustShootDirection(shootDirection, 2f);
                        break;
                }
            }

            Vector2 shootTarget = rb.position + shootDirection * 45;
            playerShooting.Shoot(shootTarget);
            shootingRequested = false;
        }

    }

    private void AdjustRewards()
    {
        float accuracy = metrics.getAccuracy();
        float parrySuccessRate = metrics.getParrySuccessRate();

        // Define boundaries for low skill
        float accuracyUpperBound = 0.70f;
        float parrySuccessUpperBound = 0.60f;
        float accuracyLowerBound = 0.30f;
        float parrySuccessLowerBound = 0.20f;
        float accuracyOptimal = accuracyUpperBound - accuracyLowerBound;
        float parryOptimal = parrySuccessUpperBound - parrySuccessLowerBound;

        float accuracyReward = CalculateReward(0.05f,accuracy, accuracyLowerBound, accuracyOptimal, accuracyUpperBound);
        float parrySuccessReward = CalculateReward(0.05f,parrySuccessRate, parrySuccessLowerBound, parryOptimal, parrySuccessUpperBound);

        AddReward(accuracyReward);
        AddReward(parrySuccessReward);
        if (accuracyReward > 0)
        {
            Debug.Log("Accuracy Reward");
        }
        else
        {
            Debug.Log("Accuracy Penalty");
        }
        if (parrySuccessReward > 0)
        {
            Debug.Log("Parry Reward");
        }
        else
        {
            Debug.Log("Parry Penalty");
        }    

    }

    void TookDamage()
    {
        AddReward(-0.1f);
    }
    void HitTarget()
    {
        AddReward(0.05f);
    }

    void Parried()
    {
        AddReward(0.05f);
    }

    void KilledEnemy(int value)
    {
        AddReward(0.02f*value);
    }

    float CalculateReward(float baseReward,float currentValue, float lowerBound, float optimal, float upperBound)
    {
        if (currentValue < lowerBound || currentValue > upperBound)
        {
            return -baseReward; 
        }
        float range = upperBound - lowerBound;
        if (range == 0) return baseReward; //Prevent division by zero
        float normalizedValue = (currentValue - lowerBound) / range; 
        float optimalNormalized = (optimal - lowerBound) / range;
        float modifier = 1 - 4 * (normalizedValue - optimalNormalized) * (normalizedValue - optimalNormalized);
        float reward = baseReward * (1 + modifier);
        return Math.Max(baseReward, Math.Min(2.5f * baseReward, reward));
    }

    private bool ValidateObservation(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void EvaluateWaveMetrics(int waveNumber, float healthLost, float completionTime)
    {

        float healthLowerBound = 0f;
        float healthOptimal = 13f;
        float healthUpperBound = 25f;
        float timeLowerBound = 6f;
        float timeOptimal = 13f;
        float timeUpperBound = 20f;

        float healthLostReward = CalculateReward(0.5f,healthLost, healthLowerBound, healthOptimal, healthUpperBound);
        float completionTimeReward = CalculateReward(0.5f, completionTime, timeLowerBound, timeOptimal, timeUpperBound);
        AddReward(healthLostReward);
        if (!this.Died)
        {
            AddReward(completionTimeReward);
        }
        if (healthLostReward > 0)
        {
            Debug.Log("Health Reward");
        }
        else
        {
            Debug.Log("Health Penalty");
        }
        if (completionTimeReward > 0)
        {
            Debug.Log("Completion Reward");
        }
        else
        {
            Debug.Log("Completion Penalty");
        }

        if (waveNumber > 1)
        {
            float averageHealthLost = metrics.getAverageHealthLostPerWave();
            float averageCompletionTime = metrics.getAverageWaveCompletionTime();

            float averageHealthLostReward = CalculateReward(0.5f, averageHealthLost, healthLowerBound, healthOptimal, healthUpperBound);
            float averageCompletionTimeReward = CalculateReward(0.5f, averageCompletionTime, timeLowerBound, timeOptimal, timeUpperBound);

            AddReward(averageHealthLostReward);
            if (averageHealthLostReward > 0)
            {
                Debug.Log("Average Health Reward");
            }
            else
            {
                Debug.Log("Average Health Penalty");
            }
            AddReward(averageCompletionTimeReward);
            if (averageCompletionTimeReward > 0)
            {
                Debug.Log("Average Completion Reward");
            }
            else
            {
                Debug.Log("Average Completion Penalty");
            }
        }
        if (this.Died)
        {
            float waveReward = (metrics.getCurrentWave() < 8) ? -6.0f : 2.0f * metrics.getCurrentWave();
            float killScoreReward = -2.0f;
            if (waveReward > 0)
            {
                killScoreReward = metrics.getKillScore() * 0.1f;
            }
            if(killScoreReward > 0)
            {
                Debug.Log("Kill Score Reward");
            }
            else
            {
                Debug.Log("Kill Score Penalty");
            }

            if (waveReward > 0)
            {
                Debug.Log("Wave Reward");
                
            }
            else
            {
                Debug.Log("Wave Penalty");
                if (metrics.getCurrentWave() > 8)
                {
                    Debug.Log("Above Wave Max");
                }
                else {
                    Debug.Log("Below Wave Min");
                }
            }

            AddReward(waveReward);
            AddReward(killScoreReward);

            Debug.Log("Death episode end");
            Debug.Log("Died at: " + metrics.getCurrentWave());
            EndEpisode();
            
        }
        if (waveNumber == 10)
        {
            AddReward(5.0f);
            Debug.Log("Win episode end");
            EndEpisode();
        }


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        useHeuristics = true;
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // Movement input
        continuousActions[0] = movementInput.x;
        continuousActions[1] = movementInput.y;

        // Dash input
        discreteActions[0] = dashRequested ? 1 : 0;

        // Parry input
        discreteActions[1] = parryRequested ? 1 : 0;

        //Shooting Input
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootingDirection = mouseWorldPosition - transform.position;
        shootingDirection.Normalize();

        continuousActions[2] = shootingDirection.x;
        continuousActions[3] = shootingDirection.y;
        discreteActions[2] = shootingRequested ? 1 : 0;

    }



    void Update()
    {
        //Heuristics
        if (useHeuristics)
        {
            movementInput.x = Input.GetAxis("Horizontal");
            movementInput.y = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dashRequested = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                parryRequested = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                shootingRequested = true;
            }
        }

    }

    private Vector2 AdjustShootDirection(Vector2 shootDirection, float adjustment)
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            Vector2 enemyPosition = nearestEnemy.transform.position;
            Vector2 playerPosition = transform.position;
            Vector2 enemyDirection = (enemyPosition - playerPosition).normalized;

            Vector2 targetPoint = enemyPosition - (enemyDirection * adjustment);

            Vector2 correctedDirection = (targetPoint - playerPosition).normalized;

            float distanceToLine = Vector2.Distance(playerPosition + shootDirection * 45, targetPoint);

            if (distanceToLine > adjustment)
            {
                return correctedDirection;
            }
        }
        return shootDirection;
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float minDistance = Mathf.Infinity;
        Vector2 position = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(enemy.transform.position, position);
            if (distance < minDistance)
            {
                nearestEnemy = enemy;
                minDistance = distance;
            }
        }

        return nearestEnemy;
    }

    private bool IsProjectileInRange(float range)
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        Vector2 playerPosition = transform.position;
        foreach (var projectile in projectiles)
        {
            float distance = Vector2.Distance(projectile.transform.position, playerPosition);
            if (distance <= range)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsDashSafe(out Vector2 dodgeDirection)
    {
        Vector2 currentPos = rb.position;
        dodgeDirection = Vector2.zero;
        GameObject nearestProjectile = FindNearestProjectile(currentPos);

        if (nearestProjectile != null)
        {
            Vector2 projectileVelocity = nearestProjectile.GetComponent<Rigidbody2D>().velocity;
            Vector2 incomingDirection = projectileVelocity.normalized;

            // Calculate perpendicular dodge directions
            Vector2 dodgeLeft = new Vector2(-incomingDirection.y, incomingDirection.x).normalized;
            Vector2 dodgeRight = new Vector2(incomingDirection.y, -incomingDirection.x).normalized;

            // Check which direction is safer
            float distanceLeft = DistanceToProjectedThreat(currentPos + dodgeLeft * playerMovement.dashDistance, nearestProjectile);
            float distanceRight = DistanceToProjectedThreat(currentPos + dodgeRight * playerMovement.dashDistance, nearestProjectile);

            if (distanceLeft > distanceRight && distanceLeft > DistanceToNearestThreat(currentPos))
            {
                dodgeDirection = dodgeLeft;
                return true;
            }
            else if (distanceRight > DistanceToNearestThreat(currentPos))
            {
                dodgeDirection = dodgeRight;
                return true;
            }
        }
        if(nearestProjectile == null)
        {
            return true;
        }
        return false;
    }

    private GameObject FindNearestProjectile(Vector2 position)
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var projectile in projectiles)
        {
            float distance = Vector2.Distance(position, projectile.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = projectile;
            }
        }
        return nearest;
    }

    private float DistanceToProjectedThreat(Vector2 position, GameObject projectile)
    {
        // Project the position of the projectile forward based on its velocity
        Vector2 futurePosition = (Vector2)projectile.transform.position + projectile.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime * 10;  // Project 10 frames ahead
        return Vector2.Distance(position, futurePosition);
    }

    private float DistanceToNearestThreat(Vector2 position)
    {
        float minDistance = Mathf.Infinity;
        Vector2 nearestThreatPosition = Vector2.zero;

        // Check for nearest projectile
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (var projectile in projectiles)
        {
            float distance = Vector2.Distance(position, projectile.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestThreatPosition = projectile.transform.position;
            }
        }

        return minDistance;
    }





}

