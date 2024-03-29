﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKnockBack
{
    public float MOVE_ACCEL = (0.11f * 60.0f);
    public float GROUND_FRICTION = 0.88f;
    public float GRAVITY = (0.08f * 60.0f); // Increased gravity value; changed to take gravity as a positive value as well
    public float FALL_GRAVITY_MULTIPLIER = 1.6f;
    public float JUMP_VEL = 0.9f;
    public float JUMP_MIN_TIME = 0.06f;
    public float JUMP_MAX_TIME = 0.20f;
    public float AIR_FALL_FRICTION = 0.98f;
    public float AIR_MOVE_FRICTION = 0.82f;

    private bool m_isBouncing = false;
    private bool m_facingRight = true;
    private bool m_jumpPressed = false;
    private bool m_jumpHeld = false;
    private bool m_wantsRight = false;
    private bool m_wantsLeft = false;

    private float m_stateTimer = 0.0f;
    private float m_coyoteTimeCounter = 0.0f;
    private float m_coyoteTime = 0.2f;
    private float m_jumpBufferTimer = 0.0f;
    private float m_jumpBufferTime = 0.1f;
    private float m_maxIdleSpeed = 1.25f;
    private float m_maxTilt = 10.0f;
    private float m_tiltSpeed = 20.0f;

    private RipplePostProcessor m_screenRipple;
    private Quaternion m_originalRotation;
    private Rigidbody2D m_rigidBody = null;
    private Vector2 m_vel = new Vector2(0, 0);
    private List<GameObject> m_groundObjects = new List<GameObject>();
    private Animator m_animator;

    private static readonly int AnimParamJump = Animator.StringToHash("Jump");
    private static readonly int AnimParamGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimParamIdleSpeed = Animator.StringToHash("IdleSpeed");
    
    [SerializeField] private ParticleSystem m_bounceParticles;
    [SerializeField] private ParticleSystem m_landParticles;

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
        m_animator = GetComponent<Animator>();
        m_originalRotation = m_rigidBody.transform.rotation;
        m_screenRipple = Camera.main.GetComponent<RipplePostProcessor>();
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

        // Set Animator parameters
        m_animator.SetBool(AnimParamJump, false);
        m_animator.SetBool(AnimParamGrounded, m_groundObjects.Count > 0);
        m_animator.SetFloat(AnimParamIdleSpeed, Mathf.Lerp(1, m_maxIdleSpeed, MOVE_ACCEL));
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


        // Set Animator parameters
        m_animator.SetBool(AnimParamJump, false);
        m_animator.SetBool(AnimParamGrounded, m_groundObjects.Count > 0);
        m_animator.SetFloat(AnimParamIdleSpeed, Mathf.Lerp(1, m_maxIdleSpeed, MOVE_ACCEL));

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
            PlayParticles(m_landParticles);
            m_isBouncing = false;
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

        // Set Animator parameters
        m_animator.SetBool(AnimParamJump, true);
        m_animator.SetBool(AnimParamGrounded, m_groundObjects.Count > 0);
        m_animator.SetFloat(AnimParamIdleSpeed, Mathf.Lerp(1, m_maxIdleSpeed, MOVE_ACCEL));
        
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
            m_isBouncing = false;
        }

        m_vel.y = 0;
        m_vel.x *= GROUND_FRICTION;
        HandleCharacterTilt();
        
        ApplyVelocity();

        if (m_groundObjects.Count == 0)
        {
            //No longer on the ground, fall.
            m_state = PlayerState.PS_FALLING;
            m_isBouncing = false;
            return;
        }

        if (m_jumpPressed || m_jumpHeld)
        {
            m_stateTimer = 0;
            m_state = PlayerState.PS_JUMPING;
            // Reset tilt to original rotation when jumping
            m_rigidBody.transform.rotation = m_originalRotation;
            m_isBouncing = false;
            return;
        }
       
        // Set Animator parameters
        m_animator.SetBool(AnimParamJump, false);
        m_animator.SetBool(AnimParamGrounded, m_groundObjects.Count > 0);
        m_animator.SetFloat(AnimParamIdleSpeed, Mathf.Lerp(1, m_maxIdleSpeed, MOVE_ACCEL));
     
    }


    private void HandleCharacterTilt()
    {
        if (m_state == PlayerState.PS_IDLE)
        {
            // Reset to original rotation when idle
            m_rigidBody.transform.rotation = Quaternion.Lerp(m_rigidBody.transform.rotation, m_originalRotation, m_tiltSpeed * Time.deltaTime);
        }
        else
        {
            // Tilt based on movement when walking
            Quaternion runningTilt = m_groundObjects.Count > 0 ? Quaternion.Euler(0, 0, m_maxTilt * m_vel.x) : Quaternion.identity;
            m_rigidBody.transform.up = Vector3.RotateTowards(m_rigidBody.transform.up, runningTilt * Vector2.up, m_tiltSpeed * Time.deltaTime, 0f);
           
        }
    }

    public void Knockback(Vector2 direction, float force)
    {
        m_vel = direction.normalized * force;
        m_state = PlayerState.PS_FALLING; // Change to the appropriate state
    }

    public bool IsBouncing()
    {
        return m_isBouncing;
    }

    public void BounceUp(float bounceForce)
    {
        if (m_state == PlayerState.PS_FALLING && m_vel.y < 0)
        {
            // If the player is falling, reduce the bounce force to simulate the effect of gravity
            float gravityEffect = (GRAVITY * FALL_GRAVITY_MULTIPLIER) * Time.fixedDeltaTime;
            Debug.Log("Gravity effect on bounce: " + gravityEffect);
            bounceForce -= gravityEffect;
        }
        m_screenRipple.RippleEffect();
        m_vel.y = bounceForce;
        m_state = PlayerState.PS_JUMPING;

        m_isBouncing = true;


        PlayParticles(m_bounceParticles);
        Debug.Log("Bounce force: " + bounceForce);
    }

    void PlayParticles(ParticleSystem particles)
    {
        if (particles != null)
        {
            particles.Play();

            // Get the local position of the particles relative to the player
            Vector3 localPos = particles.transform.localPosition;

            // Flip the local position if the player is facing left
            if (!m_facingRight)
            {
                localPos.x *= -1;
            }

            // Update the position of the particles
            particles.transform.localPosition = localPos;

            StartCoroutine(StopParticlesAfterDuration(particles.main.duration, particles));
        }
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
                    PlayParticles(m_landParticles);
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

    IEnumerator StopParticlesAfterDuration(float duration, ParticleSystem particles)
    {
        yield return new WaitForSeconds(duration);
        particles.Stop();
    }
    
    public bool IsMovingLeft()
    {
        return m_vel.x < 0;
    }

    public bool IsMovingRight()
    {
        return m_vel.x > 0;
    }
}
