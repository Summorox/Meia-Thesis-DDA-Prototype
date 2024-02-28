using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private GameObject healthBarInstance;
    public GameObject healthBarPrefab;
    public bool training = false;

    private SpriteRenderer spriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    public event Action OnTakeDamage; // Event triggered when taking damage
    public event Action OnDeath; // Event triggered on Death
    public event Action OnEnemyDeath;


    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = transform.Find("Visual").GetComponent<SpriteRenderer>();
        if (healthBarPrefab != null)
        {
            // Instantiate health bar and set it up
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarInstance.GetComponent<HealthBar>().entity = this.transform;
            healthBarInstance.GetComponent<HealthBar>().offset = new Vector3(0, 1, 0); // Adjust the offset as needed
            healthBarInstance.GetComponent<HealthBar>().healthComponent = this;
        }

    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnTakeDamage?.Invoke(); // Trigger the OnTakeDamage event
        StartCoroutine(FlashDamageEffect());
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashDamageEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public void Die()
    {
        OnDeath?.Invoke();
        if (gameObject.CompareTag("Enemy")) // Ensure this is an enemy dying
        {
            OnEnemyDeath?.Invoke();
        }
        if (!training)
        {
            Destroy(gameObject); // For now, just destroy the object.
            Destroy(healthBarInstance); // Destroy the health bar object

        }
    }

    public void Reset()
    {

        if (!training)
        {

            Destroy(gameObject); // For now, just destroy the object.
            Destroy(healthBarInstance); // Destroy the health bar object

        }
    }
}
