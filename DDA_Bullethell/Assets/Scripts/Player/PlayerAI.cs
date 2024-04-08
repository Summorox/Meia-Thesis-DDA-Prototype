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
    private bool parryRequested=false;
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
        metrics=GetComponent<PerformanceMetricsLogger>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerParrying = GetComponent<PlayerParrying>();
        playerShooting = GetComponent<PlayerShooting>();
        healthComponent = GetComponent<Health>();
        healthComponent.OnDeath += () => Died = true;
        levelManager = FindObjectOfType<LevelManager>(); // Find the LevelManager instance
        if (levelManager != null)
        {
            levelManager.OnWaveFinished += EvaluateWaveMetrics; // Subscribe to the event
        }
    }

    public override void OnEpisodeBegin()
    {
        metrics.ResetMetrics();
        this.Died = false;

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
        sensor.AddObservation(metrics.getCurrentWave());

        //Current Kill Score
        sensor.AddObservation(metrics.getKillScore());

        //Current Average Health Lost Per Wave
        if(metrics.getAverageHealthLostPerWave() != null)
        {
            sensor.AddObservation(metrics.getAverageHealthLostPerWave());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Current Average Time spent per Wave
        if (metrics.getAverageWaveCompletionTime() != null)
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
        if( enemies.Length > 0 )
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
        if (Died)
        {
            return;
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
        if (training)
        {
            AdjustRewards();
        }

    }

    private void AdjustRewards()
    {
        float accuracy = metrics.getAccuracy();
        float parrySuccessRate = metrics.getParrySuccessRate();
        float killScore = metrics.getKillScore();


        // Define boundaries for low skill
        float accuracyUpperBound = 0.3f; 
        float parrySuccessUpperBound = 0.3f; 
        float accuracyLowerBound = 0.1f; 
        float parrySuccessLowerBound = 0.0f; 

        if (accuracy > accuracyUpperBound || accuracy < accuracyLowerBound)
        {
            AddReward(-0.1f);
            Debug.Log("accuracy penalty");
        }
        else
        {
            AddReward(0.1f);
            Debug.Log("accuracy reward");
        }

        if (parrySuccessRate > parrySuccessUpperBound || parrySuccessRate < parrySuccessLowerBound)
        {
            AddReward(-0.1f);
            Debug.Log("parry penalty");
        }
        else
        {
            AddReward(0.1f);
            Debug.Log("parry reward");
        }
      
        if (this.Died)
        {
            if(metrics.getCurrentWave() > 3 || metrics.getCurrentWave() < 2)
            {
                AddReward(-1.0f);
                Debug.Log("wave penalty");
            }
            else
            {
                AddReward(1.0f);
                Debug.Log("wave reward");
            }
            if (killScore >= 0 || killScore <= 8)
            {
                AddReward(1.0f);
                Debug.Log("score reward");
            }
            else
            {
                AddReward(-1.0f);
                Debug.Log("score penalty");
            }
            EndEpisode();
        }
    }

    private void EvaluateWaveMetrics(int waveNumber, float healthLost, float completionTime)
    {
        float averageHealthLost = metrics.getAverageHealthLostPerWave();
        float averageCompletionTime = metrics.getAverageWaveCompletionTime();

        if (healthLost > 100 || healthLost < 80)
        {
            AddReward(-0.5f);
            Debug.Log("health penalty");
        }
        else
        {
            AddReward(0.5f);
            Debug.Log("health reward");
        }


        if (completionTime > 50 || completionTime < 10)
        {
            AddReward(-0.5f);
            Debug.Log("time penalty");
        }
        else
        {
            AddReward(0.5f);
            Debug.Log("time reward");
        }
        if(waveNumber > 1)
        {
            if (averageHealthLost > 100 || averageHealthLost < 80)
            {
                AddReward(-0.5f);
                Debug.Log("average health penalty");
            }
            else
            {
                AddReward(0.5f);
                Debug.Log("average health reward");
            }


            if (averageCompletionTime > 50 || averageCompletionTime < 10)
            {
                AddReward(-0.5f);
                Debug.Log("average time penalty");
            }
            else
            {
                AddReward(0.5f);
                Debug.Log("average time reward");
            }
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
