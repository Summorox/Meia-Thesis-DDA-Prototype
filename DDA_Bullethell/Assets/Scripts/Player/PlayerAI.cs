using System.Collections;
using System.Collections.Generic;
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


    private bool useHeuristics = false;
    private bool shootingRequested;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerParrying = GetComponent<PlayerParrying>();
        playerShooting = GetComponent<PlayerShooting>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add vector observations here, e.g., player position, enemy direction
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
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
        if (parryRequested)
        {
            playerParrying.Parry();
            parryRequested = false;
        }

        //Shooting
        Vector2 targetPoint = new Vector2(actions.ContinuousActions[2], actions.ContinuousActions[3]);
        targetPoint = ConvertToGameWorldPoint(targetPoint);

        if (shootingRequested)
        {
            playerShooting.Shoot(targetPoint);
            shootingRequested = false;
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
        Vector2 mousePosition = Input.mousePosition;
        continuousActions[2] = mousePosition.x;
        continuousActions[3] = mousePosition.y;
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

    private Vector2 ConvertToGameWorldPoint(Vector2 point)
    {
        // Example conversion, adjust based on your game's coordinate system
        // This could be a direct mapping or a more complex calculation based on the game's camera and UI layout
        return Camera.main.ScreenToWorldPoint(new Vector3(point.x, point.y, 0));
    }





}
