  Advanced Camera Controller System
  

 Overview
A comprehensive Unity camera system with two professional camera controllers for desktop and mobile platforms.

 Scripts Included:

    1. FreeFlyCameraController
First-person free-fly camera with mouse and keyboard controls. Perfect for scene exploration and level design.

  Features:  
- WASD movement with Q/E for up/down
- Left Shift for sprint
- Right mouse button + mouse for looking
- Arrow keys for rotation
- Smooth movement and rotation
- Mobile touch support

    2. RTSCameraController
Top-down RTS-style camera with drag panning. Ideal for strategy games and map exploration.

  Features:  
- Click and drag to pan (desktop)
- One-finger drag to pan (mobile)
- Customizable pan limits
- Smooth camera movement

 Quick Setup:

1. Import the package into your Unity project
2. Select your camera in the Hierarchy
3. Click "Add Component" and choose either:
   - FreeFlyCameraController
   - RTSCameraController
4. Adjust settings in the Inspector
5. Press Play to test

 Controls:

    FreeFly Camera
-   WASD   - Move forward/backward/left/right
-   Q/E   - Move down/up
-   Left Shift   - Sprint
-   Right Mouse   +   Mouse Move   - Look around
-   Arrow Keys   - Rotate camera

    RTS Camera
-   Left Click + Drag   (Desktop) - Pan
-   One Finger + Drag   (Mobile) - Pan




 Inspector Settings:

 FreeFlyCameraController
- Move Speed: 5 (base movement speed)
- Sprint Multiplier: 2 (speed boost when sprinting)
- Rotation Sensitivity: 2 (mouse look sensitivity)
- Keyboard Rotation Speed: 90 (degrees per second)
- Move Smoothing: 10 (higher = smoother)
- Rotate Smoothing: 10 (higher = smoother)
- Customizable arrow key bindings

 RTSCameraController
- Pan Speed: 10 (movement speed)
- Pan Limit Min: (-50, -50)
- Pan Limit Max: (50, 50)

___________________________________________________

   Code Examples:

    Basic FreeFly Setup
 csharp
public class CameraSetup : MonoBehaviour
{
    void Start()
    {
        var camera = GetComponent<FreeFlyCameraController>();
        camera.moveSpeed = 8f;
        camera.sprintMultiplier = 3f;
    }
}
______________

    Toggle Controls
=> csharp
public class CameraToggle : MonoBehaviour
{
    private FreeFlyCameraController cameraController;
    
    void Start()
    {
        cameraController = GetComponent<FreeFlyCameraController>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cameraController.enabled = !cameraController.enabled;
        }
    }
}
___________________________________________________


 Requirements:

- Unity 2019.4 or higher
- Works on: Windows, Mac, Linux, iOS, Android, WebGL
- Legacy Input Manager (included with Unity)
- No external dependencies



 Troubleshooting:

  Camera doesn't move:   Check if script is enabled in Inspector

  Mouse rotation not working:   Hold right mouse button while moving mouse

  Mobile touch not working:   Enable touch input in Player Settings

  Camera jerky:   Increase smoothing values

  RTS camera out of bounds:   Set Pan Limit Min/Max values



 Package Contents:

- FreeFlyCameraController.cs
- RTSCameraController.cs
- Demo scenes
- Documentation
- Scripts

 Version History:

  v1.0.0   - Initial release
- Free fly and RTS camera controllers
- Desktop and mobile support
- Smoothing and customization



 Support:

Email: magnusworks5@gmail.com

 LICENSE

This asset is licensed under the Unity Asset Store End User License Agreement.

For the applicable license terms, please refer to:
https://unity.com/legal/as-terms
 

  Thank you for using Advanced Camera Controller System!  
  