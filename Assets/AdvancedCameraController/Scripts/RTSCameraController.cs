using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnusWorks.CameraController;



namespace MagnusWorks.CameraController
{

/// <summary>
/// Standalone recording/showcase camera for demo scenes.
/// Provides three modes: Free Look, Orbit, and Cinematic Orbit.
/// Does not affect gameplay and does not depend on any existing scripts.
/// </summary>
/// 
public class RTSCameraController : MonoBehaviour
{
     [Header("Camera Settings")]
    public float panSpeed = 10f;
    public Vector2 panLimitMin = new Vector2(-50, -50);
    public Vector2 panLimitMax = new Vector2(50, 50);

    private Vector3 lastPanPosition;
    private bool isPanning;

    void Update()
    {
        // Only process input if this script is enabled
        if (!enabled) return;
        
        #if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
        #endif

        #if UNITY_IOS || UNITY_ANDROID
        HandleTouchInput();
        #endif
    }

    // Desktop drag support
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
            isPanning = true;
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            Vector3 delta = Input.mousePosition - lastPanPosition;
            PanCamera(delta);
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }
    }

    // Mobile drag support
    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
                isPanning = true;
            }
            else if (touch.phase == TouchPhase.Moved && isPanning)
            {
                Vector3 delta = (Vector3)touch.position - lastPanPosition;
                PanCamera(delta);
                lastPanPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isPanning = false;
            }
        }
    }

    // Move camera based on input delta
    private void PanCamera(Vector3 delta)
    {
        
        Vector3 move = new Vector3(delta.x, 0, delta.y) * panSpeed * Time.deltaTime;

        Vector3 newPos = transform.position - move;

        // Clamp the camera within limits
        newPos.x = Mathf.Clamp(newPos.x, panLimitMin.x, panLimitMax.x);
        newPos.z = Mathf.Clamp(newPos.z, panLimitMin.y, panLimitMax.y);

        transform.position = newPos;
    }
}

}