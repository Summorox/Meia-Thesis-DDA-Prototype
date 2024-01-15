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

    public event Action OnTakeDamage; // Event triggered when taking damage
    public event Action OnDeath; // Event triggered on Death



    void Start()
    {
        if(healthBarPrefab != null)
        {
            // Instantiate health bar and set it up
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBarInstance.GetComponent<HealthBar>().entity = this.transform;
            healthBarInstance.GetComponent<HealthBar>().offset = new Vector3(0, 1, 0); // Adjust the offset as needed
            healthBarInstance.GetComponent<HealthBar>().healthComponent = this;
        }
        currentHealth = maxHealth;

    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnTakeDamage?.Invoke(); // Trigger the OnTakeDamage event

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();

        Destroy(healthBarInstance); // Destroy the health bar object

        Destroy(gameObject); // For now, just destroy the object.
    }

    public void Reset()
    {

        Destroy(healthBarInstance); // Destroy the health bar object

        Destroy(gameObject); // For now, just destroy the object.
    }
}
