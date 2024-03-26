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

    public bool training = false;
    private float minX = -23, maxX = 23;
    private float minY = -12, maxY = 12;
    public Transform LevelParent;

    // Max counts
    public int maxEnemies;
    public int maxHazards;

    // Current counts
    private int currentEnemies = 0;
    private int currentHazards = 0;

    private int currentDifficultyValue=6;
    private int difficultyIncrease=2;

    public int maxWaves;

    private int waveCounter = 0;

    private bool gameStarted = false;

    public TextMeshProUGUI waveText;

    private bool playerDeath = false;


    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent
        if (training)
        {
            playerPrefab.GetComponent<PlayerMovement>().training = false;
            //playerPrefab.GetComponent<PlayerShooting>().training = this.training;
            playerPrefab.GetComponent<Health>().training = this.training;
            playerPrefab.GetComponent<Health>().OnDeath += () => playerDeath = true;
        }
    }

    

    public override void OnEpisodeBegin()
    {
        playerDeath = false;
        if (training)
        {
            ClearLevel();
            waveCounter = 0;
            //Player
            // Instantiate a new player instance
            playerPrefab.transform.position = GetRandomStartPosition();

            // Reset orientation
            playerPrefab.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());

            playerPrefab.transform.parent = LevelParent;

            //Player
            playerPrefab.GetComponent<PlayerMovement>().dead = false;
            playerPrefab.GetComponent<PlayerShooting>().dead = false;
            playerPrefab.GetComponent<BoxCollider2D>().enabled = true;

            playerPrefab.GetComponent<Health>().currentHealth = playerPrefab.GetComponent<Health>().maxHealth;
        }
        //GenerateLevel(currentDifficultyValue);
        

    }

    

    private void GenerateLevel(int DifficultyValue)
    {
        if (!training && waveCounter==0)
        {         
            GameObject player= Instantiate(playerPrefab, GetRandomStartPosition(), Quaternion.identity,LevelParent);
            player.GetComponent<Health>().OnDeath+= PlayerDeathHandler;
            player.GetComponent<Health>().currentHealth = playerPrefab.GetComponent<Health>().maxHealth;
            Debug.Log("player spawned");
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
                    bool placeHazard = randomChance > 0.00f && currentHazards < maxHazards;
                    bool placeEnemy = !placeHazard && currentEnemies < maxEnemies;

                    if (placeHazard)
                    {
                        int hazardIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
                        GameObject hazard = Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()),LevelParent);
                        currentHazards++;
                        totalDifficulty = totalDifficulty + hazard.GetComponent<EntityData>().difficultyValue;
                     }
                     else if (placeEnemy)
                     {
                        int enemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()),LevelParent);
                        currentEnemies++;
                        totalDifficulty = totalDifficulty + enemy.GetComponent<EntityData>().difficultyValue;
                    }
                    
                }
            }
            // If by the end of the loop no enemy has been placed, forcefully place one at a random position
            if (currentEnemies == 0)
            {
                Vector2 randomPos = Vector2.zero;
                while (randomPos == Vector2.zero || Physics2D.OverlapCircle(randomPos, 0.5f))
                {
                    randomPos = GetRandomStartPosition();
                }
                int enemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                //Instantiate(enemyPrefabs[0], randomPos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
            }
            gameStarted = true;
        }
        else
        {
            if (!training)
            {
                StartCoroutine(LoadMainMenu("Congratulations!"));
            }
            else
            {
                EndEpisode();
            }
        }


    }


    void ClearLevel()
    {

        foreach (Transform child in LevelParent)
        {
            if (child.CompareTag("Hazard") || (child.CompareTag("Enemy")))
                {
                Debug.Log($"Destroying {child.name} with tag {child.tag}");
                if (child.GetComponent<Health>() != null)
                {
                    Debug.Log($"Attempting to destroy health bar of {child.name}");
                    child.GetComponent<Health>().Die();
                }
                Destroy(child.gameObject); // Destroy directly for simplicity in this example
            }
        }



    }

    void Update()
    {
        if (!training && CountEnemyInstances() <= 0 && gameStarted)
        {
            gameStarted = false;
            StartCoroutine(StartNewLevel(2));
        }
        if(playerPrefab.GetComponent<Health>().currentHealth <= 0 || playerDeath) {
            if(training){
                Debug.Log("Player Death");
                EndEpisode();
            }
            else
            {
                LoadMainMenu("You Lost!");
            }
        }
    }

    private void PlayerDeathHandler()
    {

        FreezeGameEntities();

        // Load the main menu after a delay
        StartCoroutine(LoadMainMenu("You Lost!")); // Adjust delay as needed
    }

    private void FreezeGameEntities()
    {
        // Freeze enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.TryGetComponent(out Rigidbody2D rbEnemy))
            {
                rbEnemy.velocity = Vector2.zero;
                rbEnemy.isKinematic = true;
            }
            if (enemy.TryGetComponent(out Collider2D colliderEnemy))
            {
                colliderEnemy.enabled = false;
            }
        }

        // Freeze hazards
        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        foreach (GameObject hazard in hazards)
        {
            if (hazard.TryGetComponent(out Rigidbody2D rbHazard))
            {
                rbHazard.velocity = Vector2.zero;
                rbHazard.isKinematic = true;
            }
            if (hazard.TryGetComponent(out Collider2D colliderHazard))
            {
                colliderHazard.enabled = false;
            }
        }
    }

    private int CountEnemyInstances()
    {
        int enemyCount = 0;
        // Find all children with the tag "Enemy" within the given environment
        foreach (Transform child in LevelParent)
        {
            if (child.CompareTag("Enemy"))
            {
                enemyCount++;
            }
        }

        return enemyCount;
    }

    IEnumerator LoadMainMenu(String message)
    {
        // Display the congratulations message
        waveText.text = message;
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

            float x = UnityEngine.Random.Range(minX, maxX);
            float y = UnityEngine.Random.Range(minY, maxY);

            startPosition = LevelParent.position + new Vector3(x, y, 0);

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