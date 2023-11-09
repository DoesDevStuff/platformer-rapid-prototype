using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKnockBack
{
    public float MOVE_ACCEL = (0.11f * 60.0f);
    public float GROUND_FRICTION = 0.88f;

    // 1. changed to take gravity as a positive value
    public float GRAVITY = (0.08f * 60.0f); // Increased gravity value
    public float FALL_GRAVITY_MULTIPLIER = 1.6f;
    public float JUMP_VEL = 0.9f;
    public float JUMP_MIN_TIME = 0.06f;
    public float JUMP_MAX_TIME = 0.20f;
    public float AIR_FALL_FRICTION = 0.98f;
    public float AIR_MOVE_FRICTION = 0.82f;

    private Rigidbody2D m_rigidBody = null;
    private bool m_jumpPressed = false;
    private bool m_jumpHeld = false;
    private bool m_wantsRight = false;
    private bool m_wantsLeft = false;
    private float m_stateTimer = 0.0f;
    private float m_coyoteTimeCounter = 0.0f;
    private float m_coyoteTime = 0.2f;
    private float m_jumpBufferTimer = 0.0f;
    private float m_jumpBufferTime = 0.1f;
    private Vector2 m_vel = new Vector2(0, 0);
    private List<GameObject> m_groundObjects = new List<GameObject>();

    private enum PlayerState
    {
        PS_IDLE = 0,
        PS_FALLING,
        PS_JUMPING,
        PS_WALKING
    };

    private PlayerState m_state = PlayerState.PS_IDLE;

    // Use this for initialization
    void Start ()
    {
        m_rigidBody = transform.GetComponent<Rigidbody2D>();
    }

    void Update()
	{
        UpdateInput();
    }
	
    void FixedUpdate()
    {
        switch (m_state)
        {
            case PlayerState.PS_IDLE:
                Idle();
                break;
            case PlayerState.PS_FALLING:
                Falling();
                break;
            case PlayerState.PS_JUMPING:
                Jumping();
                break;
            case PlayerState.PS_WALKING:
                Walking();
                break;
            default:
                break;
        }
        // Decrease coyote timer
        m_coyoteTimeCounter -= Time.fixedDeltaTime;
        // Decrease jump buffer timer
        m_jumpBufferTimer -= Time.fixedDeltaTime;
    }

    void Idle()
    {
        m_vel = Vector2.zero;
        //Check to see whether to go into movement of some sort
        if (m_groundObjects.Count == 0)
        {
            //No longer on the ground, fall
            m_state = PlayerState.PS_FALLING;
            m_coyoteTimeCounter = 0; // Reset coyote timer when falling
            return;
        }

        //Check input for other state transitions
        if ( (m_jumpPressed || m_jumpHeld) && m_coyoteTimeCounter > 0)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            m_coyoteTimeCounter = 0; // Reset coyote timer when jumping
            return;
        }

        // Jump buffering
        if ((m_jumpPressed || m_jumpHeld) && m_jumpBufferTimer > 0)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            m_jumpBufferTimer = 0; // Reset jump buffer timer when jumping
            return;
        }

        //Test for input to move
        if (m_wantsLeft || m_wantsRight)
        {
            m_state = PlayerState.PS_WALKING;
            m_coyoteTimeCounter = 0; // Reset coyote timer when walking
            return;
        }

        // set coyote time
        m_coyoteTimeCounter = m_coyoteTime;

        // Initialize jump buffer timer
        if (m_jumpPressed)
        {
            m_jumpBufferTimer = m_jumpBufferTime;
        }
    }

    void Falling()
    {
        // Check for coyote time jump
        if ((m_jumpPressed || m_jumpHeld) && m_coyoteTimeCounter > 0)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            m_coyoteTimeCounter = 0; // Reset coyote timer when jumping
            return;
        }

        // Check for jump buffer input
        if ((m_jumpPressed || m_jumpHeld) && m_jumpBufferTimer > 0)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            m_jumpBufferTimer = 0; // Reset jump buffer timer when jumping
            return;
        }

        // since gravity is now positive we must subtract
        m_vel.y -= (GRAVITY * FALL_GRAVITY_MULTIPLIER) * Time.fixedDeltaTime;
        m_vel.y *= AIR_FALL_FRICTION;
        if (m_wantsLeft)
        {
            m_vel.x -= MOVE_ACCEL * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += MOVE_ACCEL * Time.fixedDeltaTime;
        }

        m_vel.x *= AIR_MOVE_FRICTION;

        ApplyVelocity();
    }

    void Jumping()
    {
        m_stateTimer += Time.fixedDeltaTime;

        if (m_stateTimer < JUMP_MIN_TIME || (m_jumpHeld && m_stateTimer < JUMP_MAX_TIME) )
        {
            m_vel.y = JUMP_VEL;
        }

        // subracting gravity since we're jumping
        m_vel.y -= GRAVITY * Time.fixedDeltaTime;

        if (m_vel.y <= 0)
        {
            m_state = PlayerState.PS_FALLING;
        }

        if (m_wantsLeft)
        {
            m_vel.x -= MOVE_ACCEL * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += MOVE_ACCEL * Time.fixedDeltaTime;
        }

        m_vel.x *= AIR_MOVE_FRICTION;

        ApplyVelocity();
    }

    void Walking()
    {
        if (m_wantsLeft)
        {
            m_vel.x -= MOVE_ACCEL * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += MOVE_ACCEL * Time.fixedDeltaTime;
        }
        else if (m_vel.x >= -0.05f && m_vel.x <= 0.05)
        {
            m_state = PlayerState.PS_IDLE;
            m_vel.x = 0;
        }

        m_vel.y = 0;
        m_vel.x *= GROUND_FRICTION;

        ApplyVelocity();

        if (m_groundObjects.Count == 0)
        {
            //No longer on the ground, fall.
            m_state = PlayerState.PS_FALLING;
            return;
        }

        if (m_jumpPressed || m_jumpHeld)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            return;
        }
    }

    public void Knockback(Vector2 direction, float force)
    {
        m_vel = direction.normalized * force;
        m_state = PlayerState.PS_FALLING; // Change to the appropriate state
    }

    void ApplyVelocity()
    {
        Vector3 pos = m_rigidBody.transform.position;
        pos.x += m_vel.x;
        pos.y += m_vel.y;
        m_rigidBody.transform.position = pos;
    }

    void UpdateInput()
    {
        m_wantsLeft = Input.GetKey(KeyCode.LeftArrow);
        m_wantsRight = Input.GetKey(KeyCode.RightArrow);
        m_jumpPressed = Input.GetKeyDown(KeyCode.UpArrow);
        m_jumpHeld = Input.GetKey(KeyCode.UpArrow);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
    }

    private void ProcessCollision(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
        Vector3 pos = m_rigidBody.transform.position;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            //Push back out
            Vector2 impulse = contact.normal * (contact.normalImpulse / Time.fixedDeltaTime);
            pos.x += impulse.x;
            pos.y += impulse.y;

            if (Mathf.Abs(contact.normal.y) > Mathf.Abs(contact.normal.x))
            {
                //Hit ground
                if (contact.normal.y > 0)
                {
                    if (m_groundObjects.Contains(contact.collider.gameObject) == false)
                    {
                        m_groundObjects.Add(contact.collider.gameObject);
                    }
                    if (m_state == PlayerState.PS_FALLING)
                    {
                        //If we've been pushed up, we've hit the ground.  Go to a ground-based state.
                        if (m_wantsRight || m_wantsLeft)
                        {
                            m_state = PlayerState.PS_WALKING;
                        }
                        else
                        {
                            m_state = PlayerState.PS_IDLE;
                        }
                    }
                }
                //Hit Roof
                else
                {
                    m_vel.y = 0;
                    m_state = PlayerState.PS_FALLING;
                }
            }
            else
            {
                if ((contact.normal.x > 0 && m_vel.x < 0) || (contact.normal.x < 0 && m_vel.x > 0))
                {
                    m_vel.x = 0;
                }
            }
        }
        m_rigidBody.transform.position = pos;
    }
}
