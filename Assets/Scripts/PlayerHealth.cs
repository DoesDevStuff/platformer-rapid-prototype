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
        else
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                // Instantiate blood on knockback
                GameObject bloodInstance = Instantiate(BLOOD_PREFABS[Random.Range(0, BLOOD_PREFABS.Length)], transform.position, Quaternion.identity);

                // Randomize the scale between 0.4 and 0.8 for both X and Y axes
                float randomScale = Random.Range(0.4f, 0.55f);
                bloodInstance.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                // Set the parent to player position
                bloodInstance.transform.parent = transform;

                // Get the bounds of the player's collider
                Bounds playerBounds = playerCollider.bounds;

                // Clamp the blood instance position within the player's bounds
                Vector3 clampedPosition = new Vector3(
                    Mathf.Clamp(bloodInstance.transform.position.x, playerBounds.min.x, playerBounds.max.x),
                    Mathf.Clamp(bloodInstance.transform.position.y, playerBounds.min.y, playerBounds.max.y),
                    bloodInstance.transform.position.z
                );

                // Update the blood instance position
                bloodInstance.transform.position = clampedPosition;
                StartCoroutine(FadeBloodInstance(bloodInstance, 11f));
            }
        }
    }

    // Coroutine to fade the blood instance over time
    private IEnumerator FadeBloodInstance(GameObject bloodInstance, float fadeDuration)
    {
        SpriteRenderer bloodRenderer = bloodInstance.GetComponent<SpriteRenderer>();

        if (bloodRenderer != null)
        {
            float elapsedTime = 0f;
            Color initialColor = bloodRenderer.color;

            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                bloodRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the blood is fully faded
            bloodRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

            // Destroy the blood instance after fading
            Destroy(bloodInstance);
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

    public float GetCurrentHealth()
    {
        return CURRENT_HEALTH;
    }

    public float GetMaxHealth()
    {
        return MAX_HEALTH;
    }
}