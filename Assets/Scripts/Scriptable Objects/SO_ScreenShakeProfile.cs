using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObject/ScreenShake/New CameraShake Profile")]
public class SO_ScreenShakeProfile : ScriptableObject
{
    [Header("Impulse Listener Settings")]
    public float LISTENER_AMPLITUDE = 1.0f;
    public float LISTENER_FREQUENCY = 1.0f;
    public float LISTENER_DURATION = 1.0f;

    [Header("Impulse Source Settings")]
    public float IMPULSE_TIME = 0.2f;
    public float IMPULSE_FORCE = 1.0f;
    public Vector3 DEFAULT_VELOCITY = new Vector3(0.0f, -1.0f, 0.0f);
    //Make sure to enable it in the CameraShake_manager.cs as well
    // public AnimationCurve IMPULSE_CURVE; enable this for a custom curve. it works but I like the default explosion better
}
