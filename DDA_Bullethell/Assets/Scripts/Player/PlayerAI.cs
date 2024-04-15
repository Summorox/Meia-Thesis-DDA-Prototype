using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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

    private bool Died = false;

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
            levelManager = GetComponentInParent<LevelManager>(); // Find the LevelManager instance
            if (levelManager != null)
            {
                levelManager.OnWaveFinished += EvaluateWaveMetrics; // Subscribe to the event
            }
            GetComponent<Health>().OnPlayerDeath += DeathHandler;
        }
    }

    private void DeathHandler()
    {
        this.Died = true;
        levelManager.PlayerDeathHandler();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        this.Died = false;
        metrics.ResetMetrics();

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
            playerMovement.TryDash();
            dashRequested = false;
        }

        //Parrying
        if (actions.DiscreteActions[1] == 1)
        {
            playerParrying.Parry();
            parryRequested = false;
        }

        //Shooting
        Vector2 shootDirection = new Vector2(actions.ContinuousActions[2], actions.ContinuousActions[3]).normalized;
        Vector2 shootTarget = rb.position + shootDirection * 10;

        if (actions.DiscreteActions[2] == 1)
        {
            playerShooting.Shoot(shootTarget);
            shootingRequested = false;
        }

    }

    private void AdjustRewards()
    {
        float accuracy = metrics.getAccuracy();
        float parrySuccessRate = metrics.getParrySuccessRate();

        // Define boundaries for low skill
        float accuracyUpperBound = 0.3f;
        float parrySuccessUpperBound = 0.3f;
        float accuracyLowerBound = 0.1f;
        float parrySuccessLowerBound = 0.0f;

        if (accuracy > accuracyUpperBound || accuracy < accuracyLowerBound)
        {
            AddReward(-0.05f);
        }
        else
        {
            AddReward(0.05f);
        }

        if (parrySuccessRate > parrySuccessUpperBound || parrySuccessRate < parrySuccessLowerBound)
        {
            AddReward(-0.05f);
        }
        else
        {
            AddReward(0.05f);
        }
        
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
        float averageHealthLost = metrics.getAverageHealthLostPerWave();
        float averageCompletionTime = metrics.getAverageWaveCompletionTime();
        Debug.Log("EvaluateWaveMetrics");
        if (healthLost > 90 || healthLost < 60)
        {
            AddReward(-0.5f);
        }
        else
        {
            AddReward(0.5f);
        }


        if (completionTime > 30 || completionTime < 8)
        {
            AddReward(-0.5f);
        }
        else
        {
            AddReward(0.5f);
        }
        if (waveNumber > 1)
        {
            if (averageHealthLost > 90 || averageHealthLost < 60)
            {
                AddReward(-0.5f);
            }
            else
            {
                AddReward(0.5f);
            }


            if (averageCompletionTime > 30 || averageCompletionTime < 8)
            {
                AddReward(-0.5f);
            }
            else
            {
                AddReward(0.5f);
            }
        }
        if (this.Died)
        {
            if (metrics.getCurrentWave() > 3 || metrics.getCurrentWave() < 2)
            {
                AddReward(-1.0f);
            }
            else
            {
                AddReward(1.0f);
            }
            if (metrics.getKillScore() >= 2 || metrics.getKillScore() <= 12)
            {
                AddReward(1.0f);
            }
            else
            {
                AddReward(-1.0f);
            }
            Debug.Log("Death episode end");
            EndEpisode();
        }
        if (waveNumber == 10)
        {
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





}

