using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LevelManager : Agent
{
    public GameObject[] enemyPrefabs;
    public GameObject[] hazardPrefabs;
    public GameObject playerPrefab;
    public Transform levelParent;
    public bool training = false;
    public int minX = -19, maxX = 9;
    public int minY = -9, maxY = 9;

    private Vector2 playerStartPos = new Vector2(0, 0);

    // Max counts
    public int maxEnemies;
    public int maxHazards;

    // Current counts
    private int currentEnemies = 0;
    private int currentHazards = 0;

    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent

        playerPrefab.GetComponent<PlayerMovement>().training = this.training;
        playerPrefab.GetComponent<PlayerShooting>().training = this.training;
        playerPrefab.GetComponent<Health>().training = this.training;


    }

    public override void OnEpisodeBegin()
    {

        //Player
        // Instantiate a new player instance
        playerPrefab.transform.localPosition = GetRandomStartPosition();

        // Reset orientation
        playerPrefab.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());

        //Player
        playerPrefab.GetComponent<PlayerMovement>().dead = false;
        playerPrefab.GetComponent<PlayerShooting>().dead = false;
        playerPrefab.GetComponent<BoxCollider2D>().enabled = true;



        GenerateLevel();

    }

    private void GenerateLevel()
    {
        //if (!training)
        //{
        // Place the player
        //    Instantiate(playerPrefab, playerStartPos, Quaternion.identity, levelParent);
        //}

        // Reset counts
        currentEnemies = 0;
        currentHazards = 0;


        // Generate hazards
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 pos = new Vector2(x,y);
                if (!Physics2D.OverlapCircle(pos, 3f))
                {
                    if (pos != playerStartPos) // Avoid placing hazards on the player start position
                    {
                        bool placeHazard = Random.value > 0.95f && currentHazards < maxHazards;
                        bool placeEnemy = !placeHazard && Random.value > 0.95f  && currentEnemies < maxEnemies;

                        if (placeHazard) 
                        {
                            int hazardIndex = Random.Range(0, hazardPrefabs.Length);
                            Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                            currentHazards++;
                        }
                        else if (placeEnemy) 
                        {
                            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
                            GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                            currentEnemies++;
                        }
                    }
                }
            }
        }

        // If by the end of the loop no enemy has been placed, forcefully place one at a random position
        if (currentEnemies == 0)
        {
            Vector2 randomPos = playerStartPos;
            while (randomPos == playerStartPos || Physics2D.OverlapCircle(randomPos, 0.5f))
            {
                randomPos = GetRandomStartPosition();
            }
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[enemyIndex], randomPos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
        }
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
}
