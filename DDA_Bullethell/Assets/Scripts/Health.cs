using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private HealthBar healthBar;
    public GameObject healthBarPrefab;

    public event Action OnTakeDamage; // Event triggered when taking damage
    public event Action OnDeath; // Event triggered on Death



    void Start()
    {
        if(healthBarPrefab != null)
        {
            // Instantiate health bar and set it up
            GameObject healthBarObject = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarObject.GetComponent<HealthBar>();
            healthBar.entity = this.transform;
            healthBar.offset = new Vector3(0, 1, 0); // Adjust the offset as needed
            healthBar.healthComponent = this;
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

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject); // For now, just destroy the object.
    }
}
