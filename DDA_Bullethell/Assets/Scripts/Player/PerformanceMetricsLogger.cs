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
        difficultyValue = 2;
    }

    public void LogShotFired() => shotsFired++;

    public void LogShotHit() => shotsHit++;

    public void LogParryAttempt() => parriesAttempted++;

    public void LogParrySuccess() => parriesSuccessful++;

    public void WaveCompleted(int wave, int difficultyValue, float healthLost)
    {
        currentWave = wave;
        this.difficultyValue = difficultyValue;
        metrics.averageHealthLostPerWave = (metrics.averageHealthLostPerWave * (wave - 1) + healthLost) / wave;
        float waveCompletionTime = Time.time - startTime;
        metrics.averageWaveCompletionTime = (metrics.averageWaveCompletionTime * (wave - 1) + waveCompletionTime) / wave;
        startTime = Time.time; // Reset the start time for the next wave
    }

    public void SaveMetrics(string demoName)
    {
        metrics.maxWaveReached = currentWave;
        metrics.maxDifficulty = difficultyValue; 
        metrics.averageAccuracy = shotsFired > 0 ? (float)shotsHit / shotsFired : 0;
        metrics.averageParrySuccessRate = parriesAttempted > 0 ? (float)parriesSuccessful / parriesAttempted : 0;


        string buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        // Path to the "metrics" folder within the build directory
        string metricsDirectory = Path.Combine(buildDirectory, "metrics");

        // Ensure the "metrics" directory exists
        Directory.CreateDirectory(metricsDirectory);

        // Path to the specific metrics file within the "metrics" folder
        string filePath = Path.Combine(metricsDirectory, $"{demoName}_Metrics.json");

        // Serialize the metrics object to JSON
        string json = JsonUtility.ToJson(metrics, true);

        // Write the JSON string to the file
        File.WriteAllText(filePath, json);

        Debug.Log($"Metrics saved to {filePath}");
    }
}
