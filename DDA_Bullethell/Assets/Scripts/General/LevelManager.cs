using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : Agent
{
    public GameObject[] enemyPrefabs;
    public GameObject[] hazardPrefabs;
    public GameObject[] playerTrainedPrefabs;
    public GameObject playerPrefab;


    public bool entityTraining = false;
    public bool managerTraining = false;
    private float minX = -23, maxX = 23;
    private float minY = -12, maxY = 12;
    public Transform LevelParent;

    // Max counts
    public int maxEnemies;
    public int maxHazards;

    // Current counts
    private int currentEnemies = 0;
    private int currentHazards = 0;

    private int currentDifficultyValue=2;
    private int difficultyIncrease=2;

    private bool recording = false;

    public int maxWaves;

    private int waveCounter;

    private bool gameStarted = false;

    public TextMeshProUGUI waveText;

    private DemonstrationRecorder recorder;

    private PerformanceMetricsLogger metricsLogger;

    private GameObject player;

    private int initialPlayerHealth;

    public event Action<int,float,float> OnWaveFinished;



    public override void Initialize()
    {
        base.Initialize(); // Always call the base to initialize the Agent
        if (entityTraining)
        {
            playerPrefab.GetComponent<PlayerMovement>().training = false;
            //playerPrefab.GetComponent<PlayerShooting>().training = this.training;
            playerPrefab.GetComponent<Health>().training = entityTraining;
            player = playerPrefab;
            player.GetComponent<Health>().OnPlayerDeath += PlayerDeathHandler;
        }
    }

    

    public override void OnEpisodeBegin()
    {
        ClearLevel();
        waveCounter = 1;
        if (player != null)
        {
            player.GetComponent<Health>().OnPlayerDeath -= PlayerDeathHandler;
            player.GetComponent<Health>().Die();
        }
        if (entityTraining)
        {
            //Player
            // Instantiate a new player instance
            player.transform.position = GetRandomStartPosition();

            // Reset orientation
            player.transform.rotation = Quaternion.Euler(0, 0, GetRandomStartRotation());

            player.transform.parent = LevelParent;

            //Player
            player.GetComponent<PlayerMovement>().dead = false;
            player.GetComponent<PlayerShooting>().dead = false;
            player.GetComponent<BoxCollider2D>().enabled = true;

            player.GetComponent<Health>().currentHealth = player.GetComponent<Health>().maxHealth;
            metricsLogger = player.GetComponent<PerformanceMetricsLogger>();

        }
        if (!entityTraining)
        {
            player = Instantiate(playerPrefab, GetRandomStartPosition(), Quaternion.identity, LevelParent);
            
            player.GetComponent<Health>().OnPlayerDeath += PlayerDeathHandler;
            player.GetComponent<Health>().currentHealth = playerPrefab.GetComponent<Health>().maxHealth;
            if (player.GetComponent<DemonstrationRecorder>().enabled)
            {
                recorder = player.GetComponent<DemonstrationRecorder>();
            }
            else
            {
                recorder = null;
            }
            metricsLogger = player.GetComponent<PerformanceMetricsLogger>();
            StartRecording();
            recording = true;
        }
        currentDifficultyValue = 2;
        GenerateLevel();

    }


    private void GenerateLevel()
    {
        ClearLevel();
        if (waveCounter <= maxWaves)
        {
            initialPlayerHealth = player.GetComponent<Health>().currentHealth;
            int totalDifficulty = 0;
            // Reset counts
            currentEnemies = 0;
            currentHazards = 0;
            waveText.text = "Wave " + (waveCounter);
            int mainHazardIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
            int secundaryHazardIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
            int mainEnemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            int secundaryEnemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);

            List<Vector2> spawnPositions = new List<Vector2>();

            // Generate spawn positions
            for (int i = 0; i < (maxHazards + maxEnemies) * 2; i++)
            {
                Vector2 pos = GetRandomStartPosition();
                if (!Physics2D.OverlapCircle(pos, 4f))
                {
                    spawnPositions.Add(pos);
                }
            }

            int spawnIndex = 0;

            // Generate hazards
            while (totalDifficulty < currentDifficultyValue && spawnIndex < spawnPositions.Count)
            {
                Vector2 pos = spawnPositions[spawnIndex++];

                if (totalDifficulty >= currentDifficultyValue)
                {
                    break;
                }
                if (!Physics2D.OverlapCircle(pos, 5f))
                {
                    float randomChance = UnityEngine.Random.value;
                    bool placeHazard = randomChance > 0.50f && currentHazards < maxHazards;
                    bool placeEnemy = !placeHazard && currentEnemies < maxEnemies;

                    if (placeHazard)
                    {
                        float typeFocus = UnityEngine.Random.value;
                        int hazardIndex = 0;
                        if (typeFocus > 0.30 || currentHazards <= 1)
                        {
                            hazardIndex = mainHazardIndex;
                        }
                        if (typeFocus <= 0.30 && currentHazards > 1)
                        {
                            hazardIndex = secundaryHazardIndex;
                        }
                        GameObject hazard = Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
                        currentHazards++;
                        totalDifficulty = totalDifficulty + hazard.GetComponent<EntityData>().difficultyValue;
                        hazard.GetComponent<Collider2D>().enabled = true;
                    }
                    else if (placeEnemy)
                    {
                        float typeFocus = UnityEngine.Random.value;
                        int enemyIndex = 0;
                        if (typeFocus > 0.30 || currentEnemies <= 1)
                        {
                            enemyIndex = mainEnemyIndex;
                        }
                        if (typeFocus <= 0.30 && currentEnemies > 1)
                        {
                            enemyIndex = secundaryEnemyIndex;
                        }
                        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
                        currentEnemies++;
                        totalDifficulty = totalDifficulty + enemy.GetComponent<EntityData>().difficultyValue;
                        enemy.GetComponent<Health>().OnEnemyDeath += HandleEnemyDeath;
                        enemy.GetComponent<Collider2D>().enabled = true;

                    }

                }
            }
            // If by the end of the loop no enemy has been placed, forcefully place one at a random position
            if (currentEnemies == 0)
            {
                Vector2 randomPos = Vector2.zero;
                while (randomPos == Vector2.zero || Physics2D.OverlapCircle(randomPos, 3.5f))
                {
                    randomPos = GetRandomStartPosition();
                }
                int enemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                GameObject enemy = Instantiate(enemyPrefabs[0], randomPos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
                enemy.GetComponent<Collider2D>().enabled = true;
                enemy.GetComponent<Health>().OnEnemyDeath += HandleEnemyDeath;

            }
            gameStarted = true;
        }
        else
        {
            if (!entityTraining)
            {
                StopRecording();
                StartCoroutine(LoadMainMenu("Congratulations!"));
            }
            else
            {
                OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());
                Debug.Log("Manager win episode end");
                EndEpisode();
            }
        }


    }

    private void HandleEnemyDeath(int difficultyValue)
    {
        metricsLogger.LogEnemyKill(difficultyValue);
    }

    private void StartRecording()
    {

        if (recorder != null)
        {
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd_HH-mm");
            String demoName = $"\"PlayerDemo_\"_{formattedDateTime}";
            recorder.Record = true;
            recorder.DemonstrationName = demoName;
        }
    }

    public void StopRecording()
    {
        if (recorder != null)
        {
            Debug.Log("Stopped Recording");
            recorder.Record = false;
        }
        if (metricsLogger != null && !entityTraining && recording)
        {
            Debug.Log("Save Metrics");
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd_HH-mm");
            metricsLogger.SaveMetrics("Metrics-" + formattedDateTime);

        }
    }
    void ClearLevel()
    {
        foreach (Transform child in LevelParent)
        {
            if (child.CompareTag("Enemy"))
            {
                if (child.GetComponent<Health>() != null)
                {
                    child.GetComponent<Health>().Die();
                }
                Destroy(child.gameObject);
            }
            else if (child.CompareTag("Hazard"))
            {
                Destroy(child.gameObject);
            }
        }

        GameObject[] afterimages = GameObject.FindGameObjectsWithTag("Afterimage");
        foreach (GameObject afterimage in afterimages)
        {
            Destroy(afterimage);
        }
    }

    void Update()
    {
        if (CountEnemyInstances() <= 0 && gameStarted)
        {
            gameStarted = false;
            StartCoroutine(StartNewLevel(2));
        }
        
    }

    public void PlayerDeathHandler()
    {
        FreezeGameEntities();
        if(metricsLogger != null)
        {
            metricsLogger.WaveCompleted(waveCounter, currentDifficultyValue, initialPlayerHealth);
            StopRecording();
            recording = false;
        }
        if (entityTraining)
        {
            player.GetComponent<PlayerAI>().Died = true;
            OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());
            EndEpisode();
        }
        if (!entityTraining)
        {
            // Load the main menu after a delay
            StartCoroutine(LoadMainMenu("You Lost!"));
        }
    }

    private void FreezeGameEntities()
    {
        // Freeze enemies that are children of this GameObject
        Rigidbody2D[] enemyRigidbodies = GetComponentsInChildren<Rigidbody2D>();
        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();

        foreach (Rigidbody2D rbEnemy in enemyRigidbodies)
        {
            if (rbEnemy.gameObject.CompareTag("Enemy"))
            {
                rbEnemy.velocity = Vector2.zero;
                rbEnemy.isKinematic = true;
            }
        }

        foreach (Collider2D colliderEnemy in enemyColliders)
        {
            if (colliderEnemy.gameObject.CompareTag("Enemy"))
            {
                colliderEnemy.enabled = false;
            }
        }

        // Freeze hazards that are children of this GameObject
        Rigidbody2D[] hazardRigidbodies = GetComponentsInChildren<Rigidbody2D>();
        Collider2D[] hazardColliders = GetComponentsInChildren<Collider2D>();

        foreach (Rigidbody2D rbHazard in hazardRigidbodies)
        {
            if (rbHazard.gameObject.CompareTag("Hazard"))
            {
                rbHazard.velocity = Vector2.zero;
                rbHazard.isKinematic = true;
            }
        }

        foreach (Collider2D colliderHazard in hazardColliders)
        {
            if (colliderHazard.gameObject.CompareTag("Hazard"))
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
        yield return new WaitForSeconds(2); 
                                          
        SceneManager.LoadScene("Main Menu");
    }



    IEnumerator StartNewLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(metricsLogger != null)
        {
            metricsLogger.WaveCompleted(waveCounter, currentDifficultyValue, initialPlayerHealth - player.GetComponent<Health>().currentHealth);
            if (entityTraining)
            {
                OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());
            }
        }
        currentDifficultyValue += difficultyIncrease;
        waveCounter++;
        GenerateLevel();
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
