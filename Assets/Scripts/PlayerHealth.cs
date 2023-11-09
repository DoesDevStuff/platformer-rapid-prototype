using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerHealth : MonoBehaviour
{
    public float MAX_HEALTH = 5.0f;
    public float CURRENT_HEALTH;
    public GameObject[] BLOOD_PREFABS;  // Array of blood splatter prefabs
    public GameObject DEATHPARTICLE_PREFAB;

    [SerializeField] private SO_ScreenShakeProfile m_playerShakeProfile; // expose in inspector
    private GameManager m_gameManager;
    private CinemachineImpulseSource m_impulseSource;

    // Use this for initialization
    void Start()
    {
        CURRENT_HEALTH = MAX_HEALTH;
        m_gameManager = GameObject.FindObjectOfType<GameManager>();
        m_impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    
    public void TakeDamage(float damageAmount)
    {
        // Camera shake stuff 
        //CameraShake_Manager.CAMERA_INSTANCE.CameraShake(m_impulseSource); // 1st way using default settings and no SO
        CameraShake_Manager.CAMERA_INSTANCE.ScreenShakeFromProfile(m_playerShakeProfile, m_impulseSource);

        CURRENT_HEALTH -= damageAmount;

        // Check if the player is dead
        if (CURRENT_HEALTH <= 0)
        {
            Instantiate(BLOOD_PREFABS[Random.Range(0, BLOOD_PREFABS.Length)], transform.position, Quaternion.identity);
            Die();
        }
    }

    void Die()
    {
        // Spawn death particles
        SpawnDeathParticles();

        // Disable the player GameObject
        gameObject.SetActive(false);

        // Start the coroutine from the GameManager
        m_gameManager.ReloadSceneWithDelay();
    }

    void SpawnDeathParticles()
    {
        if (DEATHPARTICLE_PREFAB != null)
        {
            Instantiate(DEATHPARTICLE_PREFAB, transform.position, Quaternion.identity);
        }
    }
}