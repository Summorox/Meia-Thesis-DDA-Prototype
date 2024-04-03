using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PerformanceMetrics
{
    public float averageHealthLostPerWave;
    public int maxWaveReached;
    public float averageWaveCompletionTime;
    public float averageAccuracy;
    public float averageParrySuccessRate;
    public float maxDifficulty;
    public float killScore;
}

public class PerformanceMetricsLogger : MonoBehaviour
{
    public PerformanceMetrics metrics = new PerformanceMetrics();
    private float startTime;
    private int shotsFired;
    private int shotsHit;
    private int parriesAttempted;
    private int parriesSuccessful;
    private int currentWave;
    private int difficultyValue;
    private int killScore;

    private void Start()
    {
        ResetMetrics();
    }

    public void ResetMetrics()
    {
        startTime = Time.time;
        shotsFired = 0;
        shotsHit = 0;
        parriesAttempted = 0;
        parriesSuccessful = 0;
        currentWave = 1;
        difficultyValue = 0;
        killScore = 0;
    }

    public void LogShotFired() => shotsFired++;

    public void LogShotHit() => shotsHit++;

    public void LogParryAttempt() => parriesAttempted++;

    public void LogParrySuccess() => parriesSuccessful++;
    public void LogEnemyKill(int value)
    {
        killScore=killScore+value;
    }

    public void WaveCompleted(int wave, int difficultyValue, float healthLost)
    {
        currentWave = wave;
        this.difficultyValue = difficultyValue;
        metrics.averageHealthLostPerWave = (metrics.averageHealthLostPerWave * (wave - 1) + healthLost) / wave;
        float waveCompletionTime = Time.time - startTime;
        metrics.averageWaveCompletionTime = (metrics.averageWaveCompletionTime * (wave - 1) + waveCompletionTime) / wave;
        startTime = Time.time; // Reset the start time for the next wave
    }

    public float getAccuracy()
    {
        return shotsFired > 0 ? (float)shotsHit / shotsFired : 0;
    }

    public float getParrySuccessRate()
    {
        return parriesAttempted > 0 ? (float)parriesSuccessful / parriesAttempted : 0;
    }

    public float getCurrentWave()
    {
        return currentWave;
    }

    public float getKillScore()
    {
        return killScore;
    }

    public float getAverageWaveCompletionTime()
    {
        return metrics.averageWaveCompletionTime;
    }

    public float getAverageHealthLostPerWave()
    {
        return metrics.averageHealthLostPerWave;
    }

    public void SaveMetrics(string demoName, int initialPlayerHealth)
    {
        metrics.maxWaveReached = currentWave;
        metrics.maxDifficulty = difficultyValue; 
        metrics.averageAccuracy = shotsFired > 0 ? (float)shotsHit / shotsFired : 0;
        metrics.averageParrySuccessRate = parriesAttempted > 0 ? (float)parriesSuccessful / parriesAttempted : 0;
        metrics.killScore = killScore;


        string buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        // Path to the "metrics" folder within the build directory
        string metricsDirectory = Path.Combine(buildDirectory, "metrics");

        // Ensure the "metrics" directory exists
        Directory.CreateDirectory(metricsDirectory);

        // Path to the specific metrics file within the "metrics" folder
        string filePath = Path.Combine(metricsDirectory, $"{demoName}_Metrics.json");

        // Check if the file already exists and generate a unique file name if necessary
        filePath = GenerateUniqueFilePath(filePath);

        // Serialize the metrics object to JSON
        string json = JsonUtility.ToJson(metrics, true);

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);

        Debug.Log($"Metrics saved to {filePath}");
    }

    private string GenerateUniqueFilePath(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        int fileCounter = 1;

        // While the file exists, append a number to make the filename unique
        while (File.Exists(filePath))
        {
            string newFilename = $"{fileNameWithoutExtension}_{fileCounter++}{extension}";
            filePath = Path.Combine(directory, newFilename);
        }

        return filePath;
    }
}
