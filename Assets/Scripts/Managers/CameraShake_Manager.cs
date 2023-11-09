using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraShake_Manager : MonoBehaviour
{
    // Singleton
    public static CameraShake_Manager CAMERA_INSTANCE;
    public float GLOBALSHAKE_FORCE = 1.0F;

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
}
