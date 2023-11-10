using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePad : MonoBehaviour {
    public float BOUNCE_FORCE = 2.0f;

    private SpriteRenderer m_sprite = null;
    private Color m_defaultColor = new Color();
    private Player m_player = null; // get reference to the player

	// Use this for initialization
	void Start () {
        m_sprite = transform.GetComponent<SpriteRenderer>();
        m_defaultColor = m_sprite.color;
        m_player = FindObjectOfType<Player>();
    }
	
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            m_player.BounceUp(BOUNCE_FORCE);
        }

        m_sprite.color = new Color(m_defaultColor.r / 2, m_defaultColor.g / 2, m_defaultColor.b / 2, 1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_sprite.color = m_defaultColor;
    }
}
