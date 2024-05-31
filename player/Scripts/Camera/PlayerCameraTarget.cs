using System;
using Godot;

public class PlayerCameraTarget : MonoBehaviour
{
    [ExportCategory("Movement")]
    [Export]
    private PlayerCamera playerCamera;

    [Export]
    private float cameraMoveSpeedFlat;

    [Export]
    private float cameraMoveSpeedVerticalMin;

    [Export]
    private float cameraMoveSpeedVerticalMax;

    [Export]
    private float distanceVerticalTillMaxSpeed;

    [Export, Range(2, 5)]
    private float verticalMoveLerpStrength;


    [ExportCategory("Settings")]
    [Export]
    private float cameraOffsetYGrounded;

    [Export]
    private float airOffsetY;

    [Export]
    private float grappleSwingOffsetY;

    [Export]
    private float distanceFromGroundToReactY;

    [Export]
    private float distanceBelowGroundToReactY;

    private PlayerController playerController;
    private Transform cameraBase;

    private Vector3 targetPosition;

    private bool playerGrounded = false;
    private bool trackPlayerInAir = false;
    private bool yOverride = false;


    private void Awake()
    {
        cameraBase = playerCamera.transform;
        playerController = FindObjectOfType<PlayerController>();

        PlayerController.OnGrounded += OnGrounded;
        PlayerController.OnAir += OnAir;
        PlayerController.OnLedgeEnter += TrackPlayerInAir;
        PlayerController.OnPole += TrackPlayerInAir;

        PlayerController.OnWallEnter += TrackPlayerInAir;
        PlayerController.OnGrapplePull += TrackPlayerInAir;
        PlayerController.OnHangingPoint += TrackPlayerInAir;

        PlayerController.OnGrappleSwing += EnterGrappleSwing;
        PlayerController.OnGrappleSwingExit += ExitGrappleSwing;
    }

    private void FixedUpdate()
    {
        CalculateTargetY();
        CalculateTargetXZ();

        GlobalPosition = targetPosition;

        MoveCameraBaseToTarget();
    }

    private void CalculateTargetXZ()
    {
        Vector3 newTargetPosition = playerController.GlobalPosition;
        newTargetPosition.Y = targetPosition.Y;

        targetPosition = newTargetPosition;
    }

    private void MoveCameraBaseToTarget()
    {
        Vector3 newCameraBasePostion = cameraBase.position;

        float flatStep = cameraMoveSpeedFlat * (float)ctx.GetPhysicsProcessDeltaTime();
        newCameraBasePostion.X = Mathf.MoveTowards(cameraBase.position.X, GlobalPosition.X, flatStep);
        newCameraBasePostion.Z = Mathf.MoveTowards(cameraBase.position.Z, GlobalPosition.Z, flatStep);

        float yDistance = Mathf.Abs(GlobalPosition.Y - cameraBase.position.Y);
        float yDistance01 = Mathf.InverseLerp(0, distanceVerticalTillMaxSpeed, yDistance);
        float ySpeed = Mathf.Lerp(cameraMoveSpeedVerticalMin, cameraMoveSpeedVerticalMax, EaseOut(yDistance01, verticalMoveLerpStrength));

        float verticalStep = ySpeed * (float)ctx.GetPhysicsProcessDeltaTime();
        newCameraBasePostion.Y = Mathf.MoveTowards(cameraBase.position.Y, GlobalPosition.Y, verticalStep);

        cameraBase.position = newCameraBasePostion;
    }

    public float EaseOut(float t, float strenght = 2)
    {
        return 1 - Mathf.Pow(1 - t, strenght);
    }

    private void CalculateTargetY()
    {
        if (yOverride) { return; }

        if (playerGrounded)
        {
            float newTargetY = playerController.GlobalPosition.Y + cameraOffsetYGrounded;

            if (this.RayCast3D(playerController.GlobalPosition, Vector3.Up, cameraOffsetYGrounded, playerCamera.groundCollisionMask, QueryTriggerInteraction.Ignore))
            {
                newTargetY = playerController.GlobalPosition.Y + airOffsetY;
            }

            float differnce = Mathf.Abs(newTargetY - targetPosition.Y);
            if (differnce < 0.05f) { return; }

            targetPosition.Y = newTargetY;
        }
        else if (
            trackPlayerInAir ||
            playerController.GlobalPosition.Y + cameraOffsetYGrounded < targetPosition.Y - distanceBelowGroundToReactY ||
            playerController.GlobalPosition.Y > targetPosition.Y + distanceFromGroundToReactY
        )
        {
            trackPlayerInAir = true;
            targetPosition.Y = playerController.GlobalPosition.Y + airOffsetY;
        }
    }

    private void OnGrounded()
    {
        playerGrounded = true;
        trackPlayerInAir = false;
    }

    private void OnAir()
    {
        playerGrounded = false;
    }

    private void TrackPlayerInAir()
    {
        trackPlayerInAir = true;
    }

    private void EnterGrappleSwing(Transform grapplePoint)
    {
        targetPosition.Y = grapplePoint.position.Y - grappleSwingOffsetY;
        yOverride = true;
    }

    private void ExitGrappleSwing()
    {
        yOverride = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        Vector3 position = playerController.GlobalPosition;
        position.Y = playerController.GlobalPosition.Y + cameraOffsetYGrounded;
        
        Gizmos.color = Color.Yellow;
        Gizmos.DrawSphere(position, .1f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, playerCamera.collisionRadius);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            if (cameraBase == null)
            {
                cameraBase = playerCamera.transform;
            }

            Vector3 position = playerController.GlobalPosition;
            position.Y += cameraOffsetYGrounded;
            GlobalPosition = position;
            cameraBase.position = position;
        }
    }
}
