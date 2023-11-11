using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dustFade : MonoBehaviour
{
    public GameObject DUST;

    private Player m_player;
    private float m_timeBtwSpawns;
    private float m_startTimeBtwSpawns = 0.05f;
    private float m_offset = -1.5f;

    private void Start()
    {
        m_player = GetComponent<Player>();
    }

    private void Update()
    {
        if (m_player != null && (m_player.IsMovingLeft() || m_player.IsMovingRight()))
        {
            // Check if the player is not bouncing
            if (!m_player.IsBouncing())
            {
                if (m_timeBtwSpawns <= 0)
                {
                    Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + m_offset, transform.position.z);
                    GameObject instance = Instantiate(DUST, spawnPosition, Quaternion.identity);
                    Destroy(instance, 5f);
                    m_timeBtwSpawns = m_startTimeBtwSpawns;
                }
                else
                {
                    m_timeBtwSpawns -= Time.deltaTime;
                }
            }
        }
    }
}
