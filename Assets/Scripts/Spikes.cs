using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public float DAMAGE_AMOUNT = 1.5f;
    public GameObject PLAYER;
    public float KNOCKBACK_FORCE = 2.0f;
    public SpriteRenderer[] PLAYER_BODY_COMPONENTS;
    public Color HURT_COLOUR;

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
            // flash when hurt
            StartCoroutine(BlinkWhenHurt());

            // Deal continuous damage here if needed
            PlayerHealth playerHealth = PLAYER.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(DAMAGE_AMOUNT); // Deal damage
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_playerInside = true;

            IKnockBack knockbackChar = collision.gameObject.GetComponent<IKnockBack>();
            if (knockbackChar != null)
            {
                Vector2 knockbackDirection = new Vector2(0.25f, 0.15f);
                knockbackChar.Knockback(knockbackDirection, KNOCKBACK_FORCE);
            }
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

    IEnumerator BlinkWhenHurt()
    {
        for (int i = 0; i < PLAYER_BODY_COMPONENTS.Length; i++)
        {
            PLAYER_BODY_COMPONENTS[i].color = HURT_COLOUR;
        }

        yield return new WaitForSeconds(0.05f);

        //reset to original
        for (int i = 0; i < PLAYER_BODY_COMPONENTS.Length; i++)
        {
            PLAYER_BODY_COMPONENTS[i].color = Color.white;
        }

    }
}
