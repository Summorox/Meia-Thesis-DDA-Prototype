using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Health healthComponent;
    public Transform entity; // The entity to which this health bar belongs
    public Vector3 offset;


    void Update()
    {

        if (entity != null) { 
            slider.value = healthComponent.currentHealth;
            // Update the position of the health bar
            slider.transform.position = entity.position + offset;
            slider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            slider.maxValue = healthComponent.maxHealth;
            UpdateColor();
        }

    }
    void UpdateColor()
    {
        float healthPercent = (float)healthComponent.currentHealth / healthComponent.maxHealth;
        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (healthPercent > 0.5f)
            fillImage.color = Color.green; // Healthy
        else if (healthPercent > 0.25f)
            fillImage.color = Color.yellow; // Warning
        else
            fillImage.color = Color.red; // Critical
    }


}
