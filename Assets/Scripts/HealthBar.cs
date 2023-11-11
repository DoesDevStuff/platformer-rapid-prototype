using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform m_bar;
    private PlayerHealth m_playerHealth;

    private void Start()
    {
        m_bar = transform.Find("Bar");
        m_playerHealth = GameObject.FindObjectOfType<PlayerHealth>();
    }

    private void Update()
    {
        // Update the health bar size based on player's current health
        float normalizedHealth = m_playerHealth.GetCurrentHealth() / m_playerHealth.GetMaxHealth();
        SetSize(normalizedHealth);
    }

    public void SetSize(float normalisedBarSize)
    {
        m_bar.localScale = new Vector3(normalisedBarSize, 1f);
    }
}
