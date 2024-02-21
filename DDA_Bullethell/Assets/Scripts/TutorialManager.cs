using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public GameObject enemyPrefab;
    private int tutorialStep = 0;
    private bool enemyDefeated = false;

    void Start()
    {
        ShowTutorial("Use WASD to move around.");
        enemyPrefab.GetComponent<Health>().OnDeath += () => enemyDefeated=true;
    }

    void Update()
    {
        switch (tutorialStep)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    NextStep("Press the left shift button to dash in the direction you are moving.");
                }
                break;
            case 1:
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    NextStep("Press the left mouse button on the screen to shoot in that direction.");
                }
                break;
            case 2:
                if (Input.GetMouseButtonDown(0)) // Left mouse button
                {
                    NextStep("Press the right mouse button to activate parrying. When active, projectiles launched at you are deflected.");
                }
                break;
            case 3:
                if (Input.GetMouseButtonDown(1)) // Right mouse button
                {
                    NextStep("Great. Now try to defeat the enemy.");
                    Invoke("SpawnEnemy", 2f);
                 
                }
                break;
            case 4:
                if (enemyDefeated) 
                {
                    NextStep("Tutorial finished! Good luck!");
                    Invoke("EndTutorial", 5f);
                }
                break;
        }
    }

    void ShowTutorial(string text)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = text;
    }

    void NextStep(string text)
    {
        tutorialStep++;
        ShowTutorial(text);
    }

    void SpawnEnemy()
    {
        enemyPrefab.SetActive(true);
    }

    void EndTutorial()
    {
        tutorialPanel.SetActive(false);
        SceneManager.LoadScene("Main Menu");
    }
}
