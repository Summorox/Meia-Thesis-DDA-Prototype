using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private HealthBar healthBar;
    public GameObject healthBarPrefab; 


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
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle death here. This could mean disabling the enemy, or ending the game for the player.
        Destroy(gameObject); // For now, just destroy the object.
    }
}
