using Godot;

public partial class PlayerCameraTarget : Node3D
{
    [ExportCategory("Movement")]
    [Export]
    private float cameraMoveSpeedFlat;

    [Export]
    private float cameraMoveSpeedVerticalMin;

    [Export]
    private float cameraMoveSpeedVerticalMax;

    [Export]
    private float distanceVerticalTillMaxSpeed;

    [Export(PropertyHint.Range, "2,5")]
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
    private PlayerCamera playerCamera;

    private Vector3 targetPosition;

    private bool playerGrounded = false;
    private bool trackPlayerInAir = false;
    private bool yOverride = false;

    public override void _Ready()
    {
        playerCamera = GetParent().GetChildByType<PlayerCamera>();
        playerController = this.GetNodeByType<PlayerController>();

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

    public override void _PhysicsProcess(double delta)
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
        Vector3 newCameraBasePostion = playerCamera.GlobalPosition;

        float flatStep = cameraMoveSpeedFlat * this.PhysicsDelta();
        newCameraBasePostion.X = Mathf.MoveToward(playerCamera.GlobalPosition.X, GlobalPosition.X, flatStep);
        newCameraBasePostion.Z = Mathf.MoveToward(playerCamera.GlobalPosition.Z, GlobalPosition.Z, flatStep);

        float yDistance = Mathf.Abs(GlobalPosition.Y - playerCamera.GlobalPosition.Y);
        float yDistance01 = Mathf.InverseLerp(0, distanceVerticalTillMaxSpeed, yDistance);
        float ySpeed = Mathf.Lerp(cameraMoveSpeedVerticalMin, cameraMoveSpeedVerticalMax, EaseOut(yDistance01, verticalMoveLerpStrength));

        float verticalStep = ySpeed * this.PhysicsDelta();
        newCameraBasePostion.Y = Mathf.MoveToward(playerCamera.GlobalPosition.Y, GlobalPosition.Y, verticalStep);

        playerCamera.GlobalPosition = newCameraBasePostion;
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

            if (this.RayCast3D(playerController.GlobalPosition, Vector3.Up, cameraOffsetYGrounded, playerCamera.groundCollisionMask))
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

    private void EnterGrappleSwing(Node3D grapplePoint)
    {
        targetPosition.Y = grapplePoint.GlobalPosition.Y - grappleSwingOffsetY;
        yOverride = true;
    }

    private void ExitGrappleSwing()
    {
        yOverride = false;
    }
}
