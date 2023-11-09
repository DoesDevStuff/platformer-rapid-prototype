using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraShake_Manager : MonoBehaviour
{
    // Singleton
    public static CameraShake_Manager CAMERA_INSTANCE;
    public float GLOBALSHAKE_FORCE = 1.0F;
    public CinemachineImpulseListener IMPLUSE_LISTENER_REF;

    private CinemachineImpulseDefinition m_impulseDef;

    private void Awake()
    {
        if (CAMERA_INSTANCE == null)
        {
            CAMERA_INSTANCE = this;
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        // we generate the camera shake from the cinemachine impulse source
        impulseSource.GenerateImpulseWithForce(GLOBALSHAKE_FORCE);
    }

    public void ScreenShakeFromProfile(SO_ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        // apply settings from scriptable object
        SetupScreenShakeSettings(profile, impulseSource);
        // Perform the screen shake
        impulseSource.GenerateImpulseWithForce(profile.IMPULSE_FORCE);
    }

    private void SetupScreenShakeSettings(SO_ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        m_impulseDef = impulseSource.m_ImpulseDefinition;

        // change the impulse listener settings
        IMPLUSE_LISTENER_REF.m_ReactionSettings.m_AmplitudeGain = profile.LISTENER_AMPLITUDE;
        IMPLUSE_LISTENER_REF.m_ReactionSettings.m_FrequencyGain = profile.LISTENER_FREQUENCY;
        IMPLUSE_LISTENER_REF.m_ReactionSettings.m_Duration = profile.LISTENER_DURATION;

        // change the impulse source settings
        m_impulseDef.m_ImpulseDuration = profile.IMPULSE_TIME;
        impulseSource.m_DefaultVelocity = profile.DEFAULT_VELOCITY;
        //m_impulseDef.m_CustomImpulseShape = profile.IMPULSE_CURVE; // enable this to get a custom curve, remember to enable in SO_ScreenShakeProfile.cs(Line 17)


    }
}
