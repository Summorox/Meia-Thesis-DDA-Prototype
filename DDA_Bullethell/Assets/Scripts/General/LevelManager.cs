using System;
using System.Collections;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Diagnostics;
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
<<<<<<< Updated upstream
    private int currentEnemies = 0;
    private int currentHazards = 0;
=======
    private float currentEnemies = 0;
    private float currentHazards = 0;
>>>>>>> Stashed changes
    private float lastEnemyCount;
    private float lastHazardsCount;
    private float lastEnemyType;
    private float lastHazardType;

<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    private int lastDifficultyValue;

    public int maxWaves;

    private int waveCounter;

    private bool gameStarted = false;

    public TextMeshProUGUI waveText;

    private DemonstrationRecorder recorder;

    private PerformanceMetricsLogger metricsLogger;

    private GameObject player;

    private int initialPlayerHealth;

    public event Action<int,float,float> OnWaveFinished;

    private bool startGame = false;

    private bool nextWave = false;

    private float lastHealthLost;

    private float lastTimeSpent;

    private float maxHealth = 250;

    private float maxKillScore = 200;

    private float maxTimePerWave = 150;

    private float maxDifficultyLevel = 32;

    private float hazardTypes = 3;

    private float enemyTypes = 4;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes


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

    public override void CollectObservations(VectorSensor sensor)
    {

        //Current Health
<<<<<<< Updated upstream
        if(player !=null)
=======
        if (player != null)
>>>>>>> Stashed changes
        {
            sensor.AddObservation((float)player.GetComponent<Health>().currentHealth / maxHealth);

        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Health lost last wave
        if (player != null)
        {
<<<<<<< Updated upstream
            sensor.AddObservation(lastHealthLost/ maxHealth);
=======
            sensor.AddObservation(lastHealthLost / maxHealth);
>>>>>>> Stashed changes

        }
        else
        {
            sensor.AddObservation(0f);
        }


        //Current Wave Reached
        sensor.AddObservation(waveCounter / maxWaves);

        //Current Kill Score
        float killScore = Utils.ValidateObservation(metricsLogger.getKillScore());
        if (killScore != -1)
        {
            sensor.AddObservation(metricsLogger.getKillScore() / maxKillScore);
        }
        else
        {
            sensor.AddObservation(0f);
        }

<<<<<<< Updated upstream
       
        //Time spent last wave
        sensor.AddObservation(lastTimeSpent/maxTimePerWave);
=======

        //Time spent last wave
        sensor.AddObservation(lastTimeSpent / maxTimePerWave);
>>>>>>> Stashed changes


        //Parry Success Rate
        float parrySuccess = Utils.ValidateObservation(metricsLogger.getParrySuccessRate());
        if (parrySuccess != -1f)
        {
            sensor.AddObservation(metricsLogger.getParrySuccessRate());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //Accuracy
        float accuracy = Utils.ValidateObservation(metricsLogger.getAccuracy());
        if (accuracy != -1f)
        {
            sensor.AddObservation(metricsLogger.getAccuracy());
        }
        else
        {
            sensor.AddObservation(0f);
        }

        //current difficulty level
        sensor.AddObservation(lastDifficultyValue / maxDifficultyLevel);

        //last enemy count
<<<<<<< Updated upstream
        sensor.AddObservation(lastEnemyCount/maxEnemies);

        //last hazard count
        sensor.AddObservation(lastHazardsCount/maxHazards);
=======
        sensor.AddObservation(lastEnemyCount / maxEnemies);

        //last hazard count
        sensor.AddObservation(lastHazardsCount / maxHazards);
>>>>>>> Stashed changes

        //last hazard type focus
        sensor.AddObservation(lastHazardType / hazardTypes);

        //last enemy type focus
<<<<<<< Updated upstream
        sensor.AddObservation(lastEnemyType/enemyTypes);
=======
        sensor.AddObservation(lastEnemyType / enemyTypes);
>>>>>>> Stashed changes

        if (startGame || nextWave)
        {
            sensor.AddObservation(1);
        }
        else
        {
            sensor.AddObservation(0);
        }

    }

    public override void OnEpisodeBegin()
    {
        ClearLevel();
        waveCounter = 1;
        lastEnemyCount = 0;
        lastHazardsCount = 0;
        lastEnemyType = 0;
        lastHazardType = 0;
        lastDifficultyValue = 0;
        lastHealthLost = 0;
        lastTimeSpent = 0;
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
            if (managerTraining)
            {
                int playerIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
<<<<<<< Updated upstream
                player= Instantiate(playerTrainedPrefabs[playerIndex], GetRandomStartPosition(), Quaternion.identity, LevelParent);
=======
                player = Instantiate(playerTrainedPrefabs[playerIndex], GetRandomStartPosition(), Quaternion.identity, LevelParent);
>>>>>>> Stashed changes
            }
            else
            {
                player = Instantiate(playerPrefab, GetRandomStartPosition(), Quaternion.identity, LevelParent);
            }
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
        }
        startGame = true;
<<<<<<< Updated upstream
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (startGame || nextWave)
        {
            Debug.Log("Wave Generated");
            int enemyCount = actions.DiscreteActions[0]+1;
            int enemyTypeFocus = actions.DiscreteActions[1];
            int hazardCount = actions.DiscreteActions[2];
            int hazardTypeFocus = actions.DiscreteActions[3];
            // Adjust level generation based on AI decisions
            GenerateLevel(enemyCount, hazardCount, enemyTypeFocus, hazardTypeFocus);
            
        }
        else
        {
            return;
        }
        
    }


    private void GenerateLevel(int enemyCount, int hazardCount, int enemyTypeFocus, int hazardTypeFocus)
    {
        ClearLevel();
        if (waveCounter <= maxWaves)
=======


    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        if (startGame || nextWave)
>>>>>>> Stashed changes
        {
            Debug.Log("Wave Generated");
            int enemyCount = actions.DiscreteActions[0] + 1;
            int enemyTypeFocus = actions.DiscreteActions[1];
            int hazardCount = actions.DiscreteActions[2];
            int hazardTypeFocus = actions.DiscreteActions[3];
            // Adjust level generation based on AI decisions
            GenerateLevel(enemyCount, hazardCount, enemyTypeFocus, hazardTypeFocus);

        }
        else
        {
            return;
        }

    }


    private void GenerateLevel(int enemyCount, int hazardCount, int enemyTypeFocus, int hazardTypeFocus)
    {
        ClearLevel();
        if (waveCounter <= maxWaves)
        {
            initialPlayerHealth = player.GetComponent<Health>().currentHealth;
            int totalDifficulty = 0;
            // Reset counts
            currentEnemies = 0;
            currentHazards = 0;
            lastHazardsCount = hazardCount;
            lastEnemyCount = enemyCount;
<<<<<<< Updated upstream
            lastHazardType = hazardTypeFocus+1;
            lastEnemyType = enemyTypeFocus+1;
            if(!managerTraining)
=======
            lastHazardType = hazardTypeFocus + 1;
            lastEnemyType = enemyTypeFocus + 1;
            if (!managerTraining)
>>>>>>> Stashed changes
            {
                waveText.text = "Wave " + (waveCounter);
            }

            // Generate hazards
<<<<<<< Updated upstream
            while(currentEnemies < enemyCount || currentHazards < hazardCount)
=======
            while (currentEnemies < enemyCount || currentHazards < hazardCount)
>>>>>>> Stashed changes
            {
                Vector2 pos = GetRandomStartPosition();
                if (!Physics2D.OverlapCircle(pos, 5f))
                {
                    float randomChance = UnityEngine.Random.value;
                    bool placeHazard = randomChance > 0.50f && currentHazards < hazardCount;
                    bool placeEnemy = !placeHazard && currentEnemies < enemyCount;

                    if (placeHazard)
                    {
                        float typeFocus = UnityEngine.Random.value;
                        int hazardIndex = UnityEngine.Random.Range(0, hazardPrefabs.Length);
<<<<<<< Updated upstream
                        if (typeFocus > 0.65 || hazardCount <=2)
=======
                        if (typeFocus > 0.65 || hazardCount <= 2)
>>>>>>> Stashed changes
                        {
                            hazardIndex = hazardTypeFocus;
                        }
                        GameObject hazard = Instantiate(hazardPrefabs[hazardIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
                        currentHazards++;
                        totalDifficulty = totalDifficulty + hazard.GetComponent<EntityData>().difficultyValue;
                        hazard.GetComponent<Collider2D>().enabled = true;
                    }
                    else if (placeEnemy)
                    {
                        float typeFocus = UnityEngine.Random.value;
                        int enemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                        if (typeFocus > 0.65 || enemyCount <= 2)
                        {
                            enemyIndex = enemyTypeFocus;
                        }
                        GameObject enemy = Instantiate(enemyPrefabs[enemyIndex], pos, Quaternion.Euler(0, 0, GetRandomStartRotation()), LevelParent);
                        currentEnemies++;
                        totalDifficulty = totalDifficulty + enemy.GetComponent<EntityData>().difficultyValue;
                        enemy.GetComponent<Health>().OnEnemyDeath += HandleEnemyDeath;
                        enemy.GetComponent<Collider2D>().enabled = true;

                    }

                }
            }
            Debug.Log("Number of enemies spawned: " + currentEnemies);
            gameStarted = true;
            startGame = false;
            nextWave = false;
            lastDifficultyValue = totalDifficulty;
        }
        else
        {
            if (!entityTraining && !managerTraining)
            {
                StopRecording();
                StartCoroutine(LoadMainMenu("Congratulations!"));
            }
            else
            {
                if (entityTraining)
                {
                    OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());

                }
                else
                {
<<<<<<< Updated upstream
                    EvaluateWave(false,true,player.GetComponent<Health>().currentHealth);
=======
                    EvaluateWave(false, true, player.GetComponent<Health>().currentHealth);
>>>>>>> Stashed changes
                    Debug.Log("Manager win episode end");
                    EndEpisode();
                }
            }
        }


    }

    private void EvaluateWave(bool playerDeath, bool playerWin, float currentHealth)
    {
        if (!playerDeath)
        {
            float healthLost = initialPlayerHealth - currentHealth;
            lastHealthLost = healthLost;
<<<<<<< Updated upstream
            if(waveCounter <=5)
            {
                if(healthLost > 25)
                {
                    AddReward(-0.02f * (healthLost - 25));
                }else if (healthLost < 25)
=======
            if (waveCounter <= 5)
            {
                if (healthLost > 25)
                {
                    AddReward(-0.02f * (healthLost - 25));
                }
                else if (healthLost < 25)
>>>>>>> Stashed changes
                {
                    AddReward(-0.50f);
                }
            }
            else
            {
                if (healthLost > 50)
                {
                    AddReward(-0.02f * (healthLost - 50));
                }
                else if (healthLost < 25)
                {
                    AddReward(-0.50f);
                }
            }

            float adjustment = 0.0f;
            float skillLevel = (float)(metricsLogger.getAccuracy() + metricsLogger.getParrySuccessRate()) / 2.0f; // Simple average of skill indicators
            float healthBasedDifficulty = (250 - currentHealth) / 25; // Number of total hits taken

            if (skillLevel >= 0.40 && healthBasedDifficulty < waveCounter - 1)
            {
                adjustment = -1.0f; // Penalize for not increasing challenge for skilled players
                Debug.Log("High Skill Penalty");
            }
            else if (skillLevel <= 0.20 && healthBasedDifficulty > waveCounter - 1)
            {
                adjustment = -1.0f; // Penalize for not decreasing challenge for unskilled players
                Debug.Log("Low Skill Penalty");
            }
            AddReward(adjustment);

        }

        float timeSpent = metricsLogger.getLastWaveCompletionTime();
        lastTimeSpent = timeSpent;
        float optimalTimeLowerBound = 5;
        float optimalTimeUpperBound = 55;
        float optimalTime = 30;
        float timeReward = Utils.CalculateRewardOptimal(0.25f, timeSpent, optimalTimeLowerBound, optimalTime, optimalTimeUpperBound);
        AddReward(timeReward);
<<<<<<< Updated upstream
        if(timeReward > 0 )
=======
        if (timeReward > 0)
>>>>>>> Stashed changes
        {
            Debug.Log("Time Reward");
        }

<<<<<<< Updated upstream
        
        

        if (playerDeath) {
            if(waveCounter <= 5)
=======



        if (playerDeath)
        {
            if (waveCounter <= 5)
>>>>>>> Stashed changes
            {
                float penalty = -1.0f * (5 - waveCounter);
                AddReward(penalty);
                Debug.Log("Death Penalty");
            }
<<<<<<< Updated upstream
            else {
=======
            else
            {
>>>>>>> Stashed changes
                float reward = 0.5f * waveCounter;
                AddReward(reward);
                Debug.Log("Death Reward");
            }

        }
        if (playerWin)
        {
<<<<<<< Updated upstream
            if(currentHealth > 50)
=======
            if (currentHealth > 50)
>>>>>>> Stashed changes
            {
                AddReward(-5f);
                Debug.Log("Win Penalty");
            }
            else
            {
                AddReward(5.0f);
                Debug.Log("Win Reward");
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
<<<<<<< Updated upstream
        if(metricsLogger != null && !managerTraining && !entityTraining)
=======
        if (metricsLogger != null && !managerTraining && !entityTraining)
>>>>>>> Stashed changes
        {
            Debug.Log("Save Metrics");
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd_HH-mm");
<<<<<<< Updated upstream
            metricsLogger.SaveMetrics("Metrics-"+formattedDateTime);
=======
            metricsLogger.SaveMetrics("Metrics-" + formattedDateTime);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======


>>>>>>> Stashed changes

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
        if (metricsLogger != null)
        {
            metricsLogger.WaveCompleted(waveCounter, lastDifficultyValue, initialPlayerHealth);
            StopRecording();
        }
        if (entityTraining)
        {
            player.GetComponent<PlayerAI>().Died = true;
            OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());
            EndEpisode();
        }
        if (managerTraining)
        {
<<<<<<< Updated upstream
            EvaluateWave(true,false,0);
=======
            EvaluateWave(true, false, 0);
>>>>>>> Stashed changes
            if (!startGame)
            {
                EndEpisode();
            }
        }
        if (!entityTraining && !managerTraining)
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
        if (metricsLogger != null)
        {
            metricsLogger.WaveCompleted(waveCounter, lastDifficultyValue, initialPlayerHealth - player.GetComponent<Health>().currentHealth);
            if (entityTraining)
            {
                OnWaveFinished?.Invoke(waveCounter, initialPlayerHealth - player.GetComponent<Health>().currentHealth, metricsLogger.getLastWaveCompletionTime());
            }
            if (managerTraining)
            {
<<<<<<< Updated upstream
                EvaluateWave(false,false,player.GetComponent<Health>().currentHealth);
=======
                EvaluateWave(false, false, player.GetComponent<Health>().currentHealth);
>>>>>>> Stashed changes
            }
        }
        nextWave = true;
        waveCounter++;
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
