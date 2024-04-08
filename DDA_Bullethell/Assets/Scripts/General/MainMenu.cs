using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject termsPanel;
    public GameObject buttonPanel;

    public static string PlayerSessionID { get; private set; }

    void Start()
    {
        // Show the terms panel by default
        termsPanel.SetActive(true);
        GeneratePlayerSessionID();
    }

    private void GeneratePlayerSessionID()
    {
        PlayerSessionID = System.Guid.NewGuid().ToString();
        Debug.Log($"Player Session ID: {PlayerSessionID}");
    }

    public void AcceptTerms()
    {
        termsPanel.SetActive(false);
        buttonPanel.SetActive(true);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Main Game");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DeclineTerms()
    {
        Application.Quit();
    }
}
