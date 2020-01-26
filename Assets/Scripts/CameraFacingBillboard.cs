using System;
using UnityEngine;

/// <summary>
/// This script is used to ensure the game object, to which it is attached, always faces the camera.
/// It is taken from here and was originally developed by Neil Carter (NCarter):
/// http://wiki.unity3d.com/index.php?title=CameraFacingBillboard
/// It has been slightly modified to satisfy the needs of the app.
/// </summary>
public class CameraFacingBillboard : MonoBehaviour
{
    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = GameObject.FindGameObjectWithTag("MainCamera")
            .GetComponent<Camera>();
    }

    /// <summary>
    /// Orient the camera after all movement is completed this frame to avoid jittering.
    /// </summary>
    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.up,
            m_Camera.transform.rotation * Vector3.forward);
    }
}