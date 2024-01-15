using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HealthBar : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    public Health healthComponent;
    public Transform entity; // The entity to which this health bar belongs
    public Vector3 offset;


    void Update()
    {
        if(entity == null)
        {
            slider.value = 0;
            Destroy(slider);
        }
        else
        {
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
        UnityEngine.UI.Image fillImage = slider.fillRect.GetComponent<UnityEngine.UI.Image>();
        if (healthPercent > 0.5f)
            fillImage.color = Color.green; // Healthy
        else if (healthPercent > 0.25f)
            fillImage.color = Color.yellow; // Warning
        else if (healthPercent > 0f)
            fillImage.color = Color.red; // Critical
    }


}
