using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public float DAMAGE_AMOUNT = 1.5f;
    public GameObject PLAYER;

    private SpriteRenderer m_sprite = null;
    private Color m_defaultColor = new Color();
    private bool m_playerInside = false;

    // Use this for initialization
    void Start()
    {
        m_sprite = transform.GetComponent<SpriteRenderer>();
        m_defaultColor = m_sprite.color;
    }

    void FixedUpdate()
    {
        if (m_playerInside)
        {
            // Deal continuous damage here if needed
            PlayerHealth playerHealth = PLAYER.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(DAMAGE_AMOUNT * Time.fixedDeltaTime); // Deal damage continuously
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_playerInside = true; // Player is inside the trigger
        }

        m_sprite.color = new Color(m_defaultColor.r / 2, m_defaultColor.g / 2, m_defaultColor.b / 2, 1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_playerInside = false; // Player has left the trigger
        }

        m_sprite.color = m_defaultColor;
    }
}
