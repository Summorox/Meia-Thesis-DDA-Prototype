using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 20;
    public string targetTag; // Tag of the target it can damage

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Check if the collided object has the correct tag
        if (hitInfo.gameObject.CompareTag(targetTag))
        {
            Debug.Log("Hit: " + hitInfo.gameObject.name);
            PlayerParrying parrying = null;
            if (hitInfo.gameObject.GetComponent<PlayerParrying>() != null)
            {
                parrying = hitInfo.gameObject.GetComponent<PlayerParrying>();
            }
            if (parrying == null || !parrying.isParrying)
            {
                Health health = hitInfo.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                Destroy(gameObject); // Destroy the projectile on hit.
            }
        }

    }
}
