using System;
using System.Collections;
using TMPro;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public int currentDifficultyValue;
    public int difficultyIncrease;

    public int maxWaves;

    private int waveCounter = 0;

    private bool gameStarted = false;

    public TextMeshProUGUI waveText;


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
        ClearLevel();
        Debug.Log(waveCounter);
        if (waveCounter < maxWaves)
        {
            int totalDifficulty = 0;
            // Reset counts
            currentEnemies = 0;
            currentHazards = 0;
            waveText.text = "Wave " + (waveCounter+1);

            // Generate hazards
            while (totalDifficulty < currentDifficultyValue)
            {
                Vector2 pos = GetRandomStartPosition();
                if (totalDifficulty >= currentDifficultyValue)
                {
                    break;
                }
                if (!Physics2D.OverlapCircle(pos, 3f))
                {
                    float randomChance = UnityEngine.Random.value;
                    bool placeHazard = randomChance > 0.65f && currentHazards < maxHazards;
                    bool placeEnemy = !placeHazard && currentEnemies < maxEnemies;

                    if (placeHazard)
                    {
                        int hazardIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
                        GameObject hazard = Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                        currentHazards++;
                        totalDifficulty = totalDifficulty + hazard.GetComponent<EntityData>().difficultyValue;
                     }
                     else if (placeEnemy)
                     {
                        int enemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
                        currentEnemies++;
                        totalDifficulty = totalDifficulty + enemy.GetComponent<EntityData>().difficultyValue;
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
                Instantiate(enemyPrefabs[0], randomPos, Quaternion.Euler(0, 0, GetRandomStartRotation()), levelParent);
            }
            gameStarted = true;
        }
        else
        {
            Debug.Log("Test");
            StartCoroutine(LoadMainMenu());
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

    void LateUpdate()
    {
        if (CountEnemyInstances() <= 0 && gameStarted)
        {
            gameStarted = false;
            StartCoroutine(StartNewLevel(2));
        }
    }

    private int CountEnemyInstances()
    {
        int enemyCount = 0;
        // Assuming all enemies are tagged as "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;

        return enemyCount;
    }

    IEnumerator LoadMainMenu()
    {
        // Display the congratulations message
        waveText.text = "Congratulations!";
        // Wait for a specified time
        yield return new WaitForSeconds(2); // Adjust the delay as needed
                                            // Load the Main Menu scene
        SceneManager.LoadScene("Main Menu");
    }



    IEnumerator StartNewLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
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
            startPosition = new Vector3(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), 0);

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
