using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEditor;

public class PlayerCamera : MonoBehaviour
{
    private enum CollisionType { Vertical, Horizontal, Diagonal };

    [ExportCategory("Input")]
    [Export]
    private Vector2 inputSensitivityMouse;

    [Export]
    private Vector2 inputSensitivityGamePad;

    [Export]
    private bool invertX;

    [Export]
    private bool invertY;

    private PlayerInput gameActions;
    private Vector2 cameraInput;
    private InputDevice inputDevice;


    [ExportCategory("Controls")]
    [Export]
    private Transform cameraLook;

    [Export]
    private Vector3 cameraLookOffset;

    [Export]
    private float cameraLookSpeed;

    [Export]
    private float clampAngleMin;

    [Export]
    private float clampAngleMax;

    private Vector3 currentCameraLookOffset;
    private float maxDistanceFromPlayer { get { return Mathf.Abs(cameraLookOffset.Z); } }


    [ExportCategory("Camera recenter")]
    [Export]
    private bool useLerp;

    [Export]
    private float recenteringSpeed;

    [Export]
    private float recenteringSpeedX;
    
    [Export]
    private float recenteringSpeedY;

    private CoroutineHandle recenterRoutine;


    [ExportCategory("Collisions")]
    [Export]
    private bool preserveCameraHeight;

    public float collisionRadius;

    public LayerMask groundCollisionMask;

    [Export]
    private LayerMask playerAndGroundCollisionMask;

    [Export, Tag]
    private string collisionIgnoreTag;

    [Export]
    private float returnToNormalAfterCollisionSpeed;

    [Export]
    private float whiskerOriginRecheckHeight;

    private Collider[] colliderBuffer;
    private Vector3 desiredCameraLookPosition;
    private CinemachineVirtualCamera vCamera;
    private PlayerController player;
    private Vector3 smoothVelocity;

    private Vector3[] verticalWhiskers;

    private void Awake()
    {
        verticalWhiskers = new Vector3[8];
        colliderBuffer = new Collider[8];

        currentCameraLookOffset = cameraLookOffset;

        vCamera = cameraLook.GetComponent<CinemachineVirtualCamera>();
        player = FindObjectOfType<PlayerController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        gameActions = ServiceLocator.Instance.Get<InputService>().inputActions;

        gameActions.actions["Camera"].performed += CameraInput;
        gameActions.actions["Camera"].canceled += CancelCamera;

        gameActions.actions["CameraRecenter"].performed += RecenterCamera;
    }

    private void OnDisable()
    {
        if (gameActions != null)
        {
            gameActions.actions["Camera"].performed -= CameraInput;
            gameActions.actions["Camera"].canceled -= CancelCamera;

            gameActions.actions["CameraRecenter"].performed -= RecenterCamera;
        }
    }

    private void FixedUpdate()
    {
        if (player.activePlayerActions.Contains(PlayerAction.Camera))
        {
            RotateBase();
        }

        MoveBase();
        RotateCameraLook();
    }

    private void CameraInput(InputAction.CallbackContext callbackContext)
    {
        cameraInput = callbackContext.ReadValue<Vector2>();
        inputDevice = callbackContext.control.device;
    }

    private void CancelCamera(InputAction.CallbackContext callbackContext)
    {
        cameraInput = Vector2.Zero;
    }

    private void RotateCameraLook()
    {
        Vector3 cameraDirection = (GlobalPosition - cameraLook.position).Normalized();

        float step = cameraLookSpeed * Time.fixedDeltaTime;
        cameraLook.forward = Vector3.Lerp(cameraLook.forward, cameraDirection, step);
    }

    private void RotateBase()
    {
        if (cameraInput.sqrMagnitude < 0.1f)
        {
            return;
        }

        CancelRecenterCamera();

        Vector2 rotation = transform.rotation.eulerAngles;

        if (rotation.X > 180)
        {
            rotation.X -= 360;
        }

        float xMultiplier = invertX ? -1 : 1;
        float yMultiplier = invertY ? -1 : 1;

        if (inputDevice is Mouse)
        {
            rotation.Y += cameraInput.X * inputSensitivityMouse.X * Time.fixedDeltaTime * xMultiplier;
            rotation.X += cameraInput.Y * inputSensitivityMouse.Y * Time.fixedDeltaTime * yMultiplier;
        }
        else if (inputDevice is Gamepad)
        {
            rotation.Y += cameraInput.X * inputSensitivityGamePad.X * Time.fixedDeltaTime * xMultiplier;
            rotation.X += cameraInput.Y * inputSensitivityGamePad.Y * Time.fixedDeltaTime * yMultiplier;
        }

        rotation.X = Mathf.Clamp(rotation.X, clampAngleMin, clampAngleMax);

        Quaternion orbitQuaternion = Quaternion.Euler(rotation.X, rotation.Y, 0.0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, orbitQuaternion, Time.fixedDeltaTime * 10);
    }

    private Vector3 CalculateDesiredCameraLookPosition()
    {
        return transform.TransformPoint(cameraLookOffset);
    }

    private void MoveBase()
    {
        desiredCameraLookPosition = CalculateDesiredCameraLookPosition();

        RaycastHit hit;

        bool anyHit = false;

        if (IsCameraInsideCollider(out hit))
        {
            HandleCameraCollision(hit);
            anyHit = true;
        }

        if (IsLineOffSightBroken(out hit))
        {
            HandleCameraCollision(hit);
            anyHit = true;
        }
        
        if (!anyHit)
        {
            desiredCameraLookPosition = transform.TransformPoint(currentCameraLookOffset);

            // Move Gradually backwards
            if (Mathf.Abs(cameraLookOffset.Z) > Mathf.Abs(currentCameraLookOffset.Z))
            {
                float step = returnToNormalAfterCollisionSpeed * Time.fixedDeltaTime;
                currentCameraLookOffset = Vector3.SmoothDamp(currentCameraLookOffset, cameraLookOffset, ref smoothVelocity, step);
            }
        }

        cameraLook.localPosition = desiredCameraLookPosition;
    }

    private bool IsLineOffSightBroken(out RaycastHit hit)
    {
        // Vertical Whiskers to detect if there is a point above or slightly below the player not obstructed by collisions (this allows player hiding behind small ledges)
        float downDistance = 2.0f;
        float upDistance = maxDistanceFromPlayer;

        if (this.RayCast3D(GlobalPosition, Vector3.Down, out RaycastHit downHit, downDistance, groundCollisionMask, QueryTriggerInteraction.Ignore))
        {
            downDistance = downHit.distance;
        }
        if (this.RayCast3D(GlobalPosition, Vector3.Up, out RaycastHit upHit, upDistance, groundCollisionMask, QueryTriggerInteraction.Ignore))
        {
            upDistance = upHit.distance;
        }

        float interval = (downDistance + upDistance) / verticalWhiskers.Length;
        Vector3 startPos = GlobalPosition + Vector3.Down * downDistance;
        for (int i = 0; i < verticalWhiskers.Length; i++)
        {
            startPos.Y += interval;
            verticalWhiskers[i] = startPos;
        }

        // Recheck higher to allow hiding behind small obstacles
        float recheckHeight = whiskerOriginRecheckHeight;
        if (this.RayCast3D(desiredCameraLookPosition, Vector3.Up, out upHit, whiskerOriginRecheckHeight, groundCollisionMask, QueryTriggerInteraction.Ignore))
        {
            recheckHeight = upHit.distance - 0.1f;
        }

        bool playerVisible = false;

        for (int count = 0; count < 2; count++)
        {
            Vector3 checkPosition = desiredCameraLookPosition;

            for (int i = 0; i < verticalWhiskers.Length; i++)
            {
                Vector3 direciton = verticalWhiskers[i] - checkPosition;
                Ray ray = new(checkPosition, direciton);

                if (!PhysicsExtension.RaycastIgnoreTag(ray, out _, direciton.Length(), groundCollisionMask, collisionIgnoreTag))
                {
                    playerVisible = true;
                    break;
                }
            }

            checkPosition.Y += recheckHeight;
        }


        // If player was spotted don't do spherecast
        if (playerVisible)
        {
            hit = new RaycastHit();
            return false;
        }

        // Do normal sphere cast check
        Vector3 direction = (desiredCameraLookPosition - GlobalPosition).Normalized();
        return PhysicsExtension.SphereCastIgnoreTag(GlobalPosition, collisionRadius, direction, out hit, maxDistanceFromPlayer, groundCollisionMask, collisionIgnoreTag);
    }

    private bool IsCameraInsideCollider(out RaycastHit hit)
    {
        Vector3 direction = (desiredCameraLookPosition - GlobalPosition).Normalized();
        Vector3 frontHitPoint = GlobalPosition;

        if (this.RayCast3D(desiredCameraLookPosition, -direction, out RaycastHit frontHit, maxDistanceFromPlayer, groundCollisionMask, QueryTriggerInteraction.Ignore))
        {
            frontHitPoint = frontHit.point;
        }

        float distance = (frontHitPoint - desiredCameraLookPosition).Length();

        if (this.RayCast3D(frontHitPoint, direction, out RaycastHit backHit, distance, groundCollisionMask, QueryTriggerInteraction.Ignore))
        {
            string ignoreTag = backHit.collider.gameObject.CompareTag(collisionIgnoreTag) ? "" : collisionIgnoreTag;

            if (backHit.distance < collisionRadius * 2)
            {
                if (PhysicsExtension.SphereCastIgnoreTag(GlobalPosition, collisionRadius, direction, out hit, distance, groundCollisionMask, ignoreTag))
                {
                    return true;
                }
            }

            if (PhysicsExtension.SphereCastIgnoreTag(backHit.point - direction * (collisionRadius * 2.0f), collisionRadius, direction, out hit, collisionRadius * 3.0f, groundCollisionMask, ignoreTag))
            {
                return true;
            }
            else if (PhysicsExtension.SphereCastIgnoreTag(frontHitPoint, collisionRadius, direction, out hit, distance, groundCollisionMask, ignoreTag))
            {
                return true;
            }
        }

        hit = new RaycastHit();
        return false;
    }

    private void HandleCameraCollision(in RaycastHit hit)
    {
        Vector3 surfaceNormal;
        if (hit.collider.Raycast(new Ray(hit.point + 0.3f * hit.normal, -hit.normal), out RaycastHit result, 0.5f))
        {
            surfaceNormal = result.normal;
        }
        else
        {
            surfaceNormal = hit.normal;
        }

        // Check if colission is vertical, horizontal or diagonal
        float absoluteDotProduct = Mathf.Abs(Vector3.Dot(surfaceNormal, Vector3.Up));

        CollisionType collisionType = absoluteDotProduct < 0.1f ? CollisionType.Horizontal : 
                                      absoluteDotProduct < 0.9f ? CollisionType.Diagonal : 
                                      CollisionType.Vertical;

        Vector3 newCameraPosition = hit.point + hit.normal * collisionRadius;

        if (preserveCameraHeight)
        {
            if (collisionType == CollisionType.Horizontal)
            {
                Vector3 desiredPreserveHeightPosition = newCameraPosition;
                desiredPreserveHeightPosition.Y = desiredCameraLookPosition.Y;

                Vector3 newCameraXZBaseY = new(newCameraPosition.X, GlobalPosition.Y, newCameraPosition.Z);
                float distance = Mathf.Abs(newCameraXZBaseY.Y - desiredPreserveHeightPosition.Y);

                Vector3 rayDirection = desiredPreserveHeightPosition.Y < newCameraPosition.Y ? Vector3.Down : Vector3.Up;
                Debug.DrawRay(newCameraXZBaseY, rayDirection * (maxDistanceFromPlayer + collisionRadius), Color.red);

                if (this.RayCast3D(newCameraXZBaseY, rayDirection, out RaycastHit hitInfo, maxDistanceFromPlayer + collisionRadius, groundCollisionMask, QueryTriggerInteraction.Ignore))
                {
                    float hitDistance = Mathf.Abs(newCameraXZBaseY.Y - hitInfo.point.Y);
                    if (hitDistance > distance)
                    {
                        newCameraPosition = desiredPreserveHeightPosition;
                    }
                    else
                    {
                        newCameraPosition = hitInfo.point + -rayDirection * collisionRadius;
                    }
                }
                else
                {
                    newCameraPosition = desiredPreserveHeightPosition;
                }
            }
        }

        currentCameraLookOffset = transform.InverseTransformPoint(newCameraPosition);
        currentCameraLookOffset.Y = cameraLookOffset.Y;
        currentCameraLookOffset.X = cameraLookOffset.X;

        desiredCameraLookPosition = newCameraPosition;
    }


    private void RecenterCamera(InputAction.CallbackContext callbackContext)
    {
        if (useLerp) { recenterRoutine = StartCoroutine(RecenteringCameraLerp()); }
        else { recenterRoutine = StartCoroutine(RecenteringCamera()); }
    }

    private void CancelRecenterCamera()
    {
        if (recenterRoutine != null)
        {
            StopCoroutine(recenterRoutine);
            recenterRoutine = null;
        }
    }

    private IEnumerator RecenteringCameraLerp()
    {
        Vector3 startForward = Transform.Forward();
        Vector3 endForward = player.Transform.Forward();

        YieldInstruction yieldInstruction = new WaitForFixedUpdate();

        float angle = Vector3.Angle(Transform.Forward(), player.Transform.Forward());
        float duration = angle / recenteringSpeed;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime / duration;

            Transform.Forward() = Vector3.Lerp(startForward, endForward, Mathf.Pow(t, 1.5f));

            yield return yieldInstruction;
        }

        recenterRoutine = null;
    }

    private IEnumerator RecenteringCamera()
    {
        YieldInstruction yieldInstruction = new WaitForFixedUpdate();

        while (!transform.localRotation.eulerAngles.ApproximatelyEqual(player.transform.eulerAngles))
        {
            float yStep = recenteringSpeedX * Time.fixedDeltaTime;
            float yRotation = Mathf.MoveTowards(transform.rotation.eulerAngles.Y, player.transform.eulerAngles.Y, yStep);

            float xStep = recenteringSpeedY * Time.fixedDeltaTime;
            float xRotation = Mathf.MoveTowards(transform.rotation.eulerAngles.X, player.transform.eulerAngles.X, xStep);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0.0f);

            yield return yieldInstruction;
        }

        recenterRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cameraLook.position, collisionRadius);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            cameraLook.position = CalculateDesiredCameraLookPosition();
        }
    }
}
