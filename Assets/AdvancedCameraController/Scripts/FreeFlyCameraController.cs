using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnusWorks.CameraController;



namespace MagnusWorks.CameraController
{

public class FreeFlyCameraController : MonoBehaviour
{
     
    #region Inspector Fields

    [Header("Free Look Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Sprint speed multiplier when holding Left Shift.")]
    [SerializeField] private float sprintMultiplier = 2f;

    [Tooltip("Mouse rotation sensitivity.")]
    [SerializeField] private float rotationSensitivity = 2f;

    [Tooltip("Keyboard rotation speed (degrees per second).")]
    [SerializeField] private float keyboardRotationSpeed = 90f;

    [Tooltip("Smoothing factor for movement (higher = smoother).")]
    [SerializeField] private float moveSmoothing = 10f;

    [Tooltip("Smoothing factor for rotation (higher = smoother).")]
    [SerializeField] private float rotateSmoothing = 10f;
    

    [Header("Keyboard Rotation Keys (Free Look)")]
    [Tooltip("Key to rotate camera left.")]
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.LeftArrow;

    [Tooltip("Key to rotate camera right.")]
    [SerializeField] private KeyCode rotateRightKey = KeyCode.RightArrow;

    [Tooltip("Key to rotate camera up.")]
    [SerializeField] private KeyCode rotateUpKey = KeyCode.UpArrow;

    [Tooltip("Key to rotate camera down.")]
    [SerializeField] private KeyCode rotateDownKey = KeyCode.DownArrow;
    
    // Input flags
    private bool isRotating = false;
    #endregion


    #region Unity Lifecycle

    void Update()
    {
        // Calling UpdateFreeLook()
        UpdateFreeLook();

    }

    #endregion


    #region Free Look Function

    private void UpdateFreeLook()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        if (Input.GetKey(KeyCode.Q)) upDown = -1f;
        if (Input.GetKey(KeyCode.E)) upDown = 1f;

        // Calculate movement direction relative to camera orientation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        // Normalize horizontal movement to prevent faster diagonal movement
        Vector3 moveDirection = (forward * vertical + right * horizontal + up * upDown).normalized;

        // Apply speed and sprint multiplier
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        // Apply smooth movement
        Vector3 targetPosition = transform.position + moveDirection * currentSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSmoothing * Time.deltaTime);

        // Handle rotation - Mouse
        float mouseX = 0f;
        float mouseY = 0f;
        
        if (Input.GetMouseButton(1))
        {
            mouseX = Input.GetAxis("Mouse X") * rotationSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity;
        }

        // Handle rotation - Keyboard
        float keyboardX = 0f;
        float keyboardY = 0f;
        
        if (Input.GetKey(rotateLeftKey)) keyboardX = -keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(rotateRightKey)) keyboardX = keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(rotateUpKey)) keyboardY = keyboardRotationSpeed * Time.deltaTime;
        if (Input.GetKey(rotateDownKey)) keyboardY = -keyboardRotationSpeed * Time.deltaTime;

        // Combine mouse and keyboard rotation
        float totalX = mouseX + keyboardX;
        float totalY = mouseY + keyboardY;

        if (totalX != 0f || totalY != 0f)
        {
            Vector3 targetRotation = transform.eulerAngles + new Vector3(-totalY, totalX, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(targetRotation),
                rotateSmoothing * Time.deltaTime
            );
        }
    }

    #endregion

}

}