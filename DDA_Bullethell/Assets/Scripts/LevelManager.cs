using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public int currentDifficultyValue = 5;
    public int difficultyIncrease = 5;

    private int waveCounter = 0;

    private int aliveEnemies = 0;

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

        GenerateLevel(currentDifficultyValue);

    }

    private void GenerateLevel(int DifficultyValue)
    {
        if (!training && waveCounter==0)
        {
            Instantiate(playerPrefab, playerStartPos, Quaternion.identity, levelParent);
        }
        if (waveCounter == 5)
        {
            SceneManager.LoadScene("Main Menu");
        }
        ClearLevel();
        int totalDifficulty = 0;
        // Reset counts
        currentEnemies = 0;
        currentHazards = 0;


        // Generate hazards
        while (totalDifficulty < currentDifficultyValue)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    if(totalDifficulty >= currentDifficultyValue)
                    {
                        break;
                    }
                    if (!Physics2D.OverlapCircle(pos, 3f))
                    {
                        if (pos != playerStartPos) // Avoid placing hazards on the player start position
                        {
                            bool placeHazard = Random.value > 0.95f && currentHazards < maxHazards;
                            bool placeEnemy = !placeHazard && Random.value > 0.95f && currentEnemies < maxEnemies;

                            if (placeHazard)
                            {
                                int hazardIndex = Random.Range(0, hazardPrefabs.Length);
                                GameObject hazard=Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                                currentHazards++;
                                totalDifficulty = totalDifficulty + hazard.GetComponent<EntityData>().difficultyValue;
                            }
                            else if (placeEnemy)
                            {
                                int enemyIndex = Random.Range(0, enemyPrefabs.Length);
                                GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                                currentEnemies++;
                                aliveEnemies++;
                                enemy.GetComponent<Health>().OnEnemyDeath += HandleEnemyDeath;
                                totalDifficulty = totalDifficulty + enemy.GetComponent<EntityData>().difficultyValue;
                            }
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

    private void ClearLevel()
    {
        foreach (Transform child in levelParent)
        {
            if (child.CompareTag("Hazard"))
            {
                if (child.GetComponent<Health>() != null)
                {
                    child.GetComponent<Health>().Die();
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    void HandleEnemyDeath()
    {
        Debug.Log("Enemy Death");
        aliveEnemies--;
        if (aliveEnemies <= 0)
        {
            StartNewLevel();
        }
    }

    private void StartNewLevel()
    {
        currentDifficultyValue += difficultyIncrease;
        waveCounter++;
        GenerateLevel(currentDifficultyValue);
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
