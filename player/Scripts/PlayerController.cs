using PlayerStates;
using System.Collections.Generic;
using Godot;
using MEC;

public partial class PlayerController : CharacterBody3D
{
    public const float GRAVITY = -9.81f;

    public bool UseGravity { get; set; } = true;
    public Vector3 InputDirection { get; private set; }
    public bool SlopeCheckHasHit { get; private set; }
    public Vector3 SlopeNormal { get; set; }
    public float SlopeAngle { get; set; }
    public Vector2 MovementInput { get; private set; }
    public Vector3 VisualsDirection { get; set; }
    public CollisionObject3D PoleTopCollider { get; private set; }
    public RaycastHit3D ledgeDownHit { get; private set; }
    public RaycastHit3D ledgeForwardHit { get; private set; }
    public Vector3 ledgeDirection { get; private set; }
    public float PoleStartHeight { get; private set; }
    public Node3D Pole { get; private set; }
    public Vector3 WallNormal { get; private set; }
    public Vector3 ClimbDirection { get; private set; }
    public CollisionShape3D PlayerCollider { get; private set; }
    public Camera3D MainCamera { get; private set; }
    public Vector3 SolarDiveDirection { get; set; }


    [Export]
    private bool debugStateMachine;

    [Export]
    public float playerHeight = 0.5f;

    [Export]
    public Node3D visuals;

    private StateMachine<PlayerController> stateMachine;

    [ExportCategory("Velocity")]
    [Export]
    public float maxVelocity = 6;
    public float MaxVelocity { get { return maxVelocity; } set { maxVelocity = Mathf.Clamp(value, 0, 40); } }

    [Export]
    public float moveSpeed;

    public float defaultMaxVelocity;

    private float inputVelocityDotProduct;

    public Dictionary<PlayerVelocitySource, float> velocityLibrary = new();
    public Dictionary<PlayerVelocitySource, bool> maintainVelocityLibrary = new();
    public PlayerVelocitySource sourceToRemove = PlayerVelocitySource.none;


    [ExportCategory("Slope")]
    // the Max Angle the player can normaly walk on
    [Export]
    public float maxSlopeAngle;

    [Export]
    public float slopeJumpHeight;


    [ExportCategory("Ground")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint GroundLayer;

    public bool steppedLastFrame;


    [ExportCategory("Rotation")]
    // rotation at Max player velocity
    [Export]
    private float rotationSmoothMax = 0.1f;
    // rotation at Min player velocity
    [Export]
    private float rotationSmoothMin = 1;

    [Export]
    public bool lockRotation;

    // Any dot product lower then this is a full direction change
    private const float fullDirectionChangeCheck = -0.5f;


    [ExportCategory("Jumping")]
    // the total amount of jumps you have, 1 for the ground the rest for the air
    [Export]
    public int MaxJumps = 1;

    [Export]
    public float jumpForce = 600;

    [Export]
    public float maxJumpTime = 0.5f;

    public float fallingHeight;

    public bool jumpPressed;
    public bool jumpPressCanTrigger;

    public CoroutineHandle jumpCoroutine;

    private Internal.Timer jumpInputTimer;
    public Internal.Timer coyoteTimer;
    public Internal.Timer postJumpGroundCheckDelayTimer;

    [ExportCategory("Slide")]
    [Export]
    public Node3D bottomVisualsPivot;

    public bool slideDivePressed;
    public bool slideDivePressCanTrigger;
    public bool slideCoyoteTime;

    public Internal.Timer slideInputTimer;
    public Internal.Timer slideTimer;


    [ExportCategory("Dive")]
    [Export]
    public int divesLeftDefault = 1;

    public int divesLeft;


    // --- Ground Pound ---
    private bool groundPoundPressed;
    public bool groundPoundPressCanTrigger;

    public Internal.Timer groundPoundLandTimer;
    public Internal.Timer solarDiveTimer;


    [ExportCategory("Ledge Hanging")]
    [Export]
    public bool ledgeIsHangingPoint;

    public float ledgeMoveBackTime = 0;
    public float ledgeMoveForwardTime = 0;

    public Internal.Timer ledgeGrabCoolDown;


    [ExportCategory("Poles")]
    [Export(PropertyHint.Layers3DPhysics)]
    public uint PoleLayer;

    [Export]
    private string poleTopTag;

    [Export]
    public string poleTag;

    public const float POLECLIMBSPEED = 4;
    public const float POLETURNSPEED = 4;
    public const float POLEDISTANCE = 0.7f;
    public Internal.Timer poleLockTimer;

    [ExportCategory("WallJumping")]
    [Export]
    private float climbingMaxDurration = 0.7f;

    private int maxClimbsSinceGrounded = 3;

    public bool canClimb;
    public int wallClimbsSinceGrounded;
    public float wallMoveBackTime;
    public Vector3 oldWallNormal;

    public CoroutineHandle climbRotateCoroutine;
    public Internal.Timer climbingTimer;

    [ExportCategory("Grappling")]
    [Export(PropertyHint.Layers3DPhysics)]
    private uint grappleable; // layers that can be grappled

    [Export(PropertyHint.Layers3DPhysics)]
    private uint grappleCheckLayers; //layers that can block grapples

    [Export]
    private Node3D selectedPointVisulizer;

    [Export]
    private Node3D topPivot;

    [Export]
    private float minDistanceCamera = 8.5f;

    [Export]
    public float grappleMaxDistance = 30;

    [Export]
    public float grappleMinDistance = 2;

    [Export]
    public float swingSpeed = 5;

    [Export]
    public float desiredTimeToReachGrapple = 0.5f;

    private bool grapplePressed;
    private bool grapplePressCanTrigger;
    private float grappleDelayTime = 0.3f;
    
    private Node3D selectionGrapplePoint;

    public bool grapplePullHasReachedDestination;
    
    public Node3D activeGrapplePoint;
    public Node3D lastGrappleObject;
    public Internal.Timer grappleCooldownTimer;

    private CollisionObject3D[] grapplePointColliders;

    [ExportCategory("Swing")]
    [Export]
    private string swingPointTag;

    public Node3D currentGrapplePoint;
    
    public Internal.Timer swingLockTimer;

    public override void _Ready()
    {
        GetReferences();
        SetupProperties();
        StateMachineSetup();
        SetupCharacterInput();
        InitializeVelocitySystem();
        SetupPlayerEvents();
    }


    public override void _ExitTree()
    {
        DesubscribeCharacterInput();
    }

    private void SetupProperties()
    {
        VisualsDirection = Transform.Forward();
        defaultMaxVelocity = MaxVelocity;

        jumpInputTimer = new Internal.Timer(0.1f);
        coyoteTimer = new Internal.Timer(0.2f, () => slideCoyoteTime = false);
        groundPoundLandTimer = new Internal.Timer(0.3f);
        postJumpGroundCheckDelayTimer = new Internal.Timer(0.1f);
        slideInputTimer = new Internal.Timer(0.1f);
        slideTimer = new Internal.Timer(0.4f);
        ledgeGrabCoolDown = new Internal.Timer(0.3f);
        poleLockTimer = new Internal.Timer(0.13f);
        climbingTimer = new Internal.Timer(climbingMaxDurration);
        grappleCooldownTimer = new Internal.Timer(0.5f);
        swingLockTimer = new Internal.Timer(0.3f);
        solarDiveTimer = new Internal.Timer(0.5f);

        grapplePointColliders = new CollisionObject3D[20];
    }

    private void GetReferences()
    {
        PlayerCollider = this.GetChildByType<CollisionShape3D>();
        MainCamera = this.GetNodeByType<Camera3D>();
    }

    private void StateMachineSetup()
    {
        stateMachine = new StateMachine<PlayerController>(
            this,
            new RootState<PlayerController>[]
            {
                 new GroundedRoot(),
                 new AirRoot(),
                 new OnObjectRoot()
            },
            new Walk(),
            new Idle(),
            new GroundPoundLand(),
            new DirectionChange(),
            new AirMovement(),
            new GroundPound(),
            new NormalJump(),
            new AirNormalJump(),
            new PostJump(),
            new BackJump(),
            new GroundPoundJump(),
            new SlopeSlide(),
            new Slide(),
            new Dive(),
            new SlopeSlideJump(),
            new SlideJump(),
            new SlideEnd(),
            new LedgeGrab(),
            new LedgeJump(),
            new OnPoleTop(),
            new OnPole(),
            new PoleJump(),
            new WallJump(),
            new WallJumpLow(),
            new WallSlideDown(),
            new WallSlideUp(),
            new GrapplePull(),
            new GrappleSwing(),
            new Swing(),
            new SolarDive()
        )
        {
            DebugEnabled = debugStateMachine
        };

        SetupTransitions();
        stateMachine.ChangeState(typeof(GroundedRoot));

    }

    public override void _PhysicsProcess(double delta)
    {
        SlopeChecker();
        UpdateInputDirection();

        stateMachine.OnPhysicsUpdate();

        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        //Type currentStateType = stateMachine.currentState.GetType();
        //if (currentStateType != typeof(PostJump) && currentStateType != typeof(AirMovement) && currentStateType != typeof(Walk) && currentStateType != typeof(Idle))
        //{
        //    DisableGrapplePoint();
        //}

        UpdateVelocitySystem();
        stateMachine.OnUpdate();
    }

    private void UpdateInputDirection()
    {
        // Get relative forward direction by finding the distance between the camera and player.
        Vector3 viewDir = GlobalPosition - new Vector3(MainCamera.GlobalPosition.X, MainCamera.GlobalPosition.Y, MainCamera.GlobalPosition.Z);

        Vector3 cameraForward = viewDir.Normalized();
        Vector3 cameraRight = new Quaternion(Vector3.Up, 90)  * cameraForward;

        // Get the combined direction of the horizontal and vertical inputs.
        InputDirection = cameraForward * MovementInput.Y + cameraRight * MovementInput.X;
        Vector3 flatVelocity = GetFlatVelocity();

        inputVelocityDotProduct = InputDirection.Dot(flatVelocity.Normalized());
        if (Mathf.Abs(flatVelocity.X) < 0.1f && Mathf.Abs(flatVelocity.Z) < 0.1f)
        {
            inputVelocityDotProduct = 1;
        }
    }

    private void SlopeChecker()
    {
        SlopeAngle = VectorExtensions.Angle(SlopeNormal, Vector3.Up);
        SlopeNormal = Vector3.Up;
        SlopeCheckHasHit = true;
        return;

        Vector3 startPosition = GlobalPosition + new Vector3(0f, playerHeight, 0f);
        Vector3 endPosition = startPosition + Vector3.Down * 5f;

        DebugDraw3D.DrawArrow(startPosition, endPosition, Color.Color8(0, 255, 0), 0.1f, false, 0.5f);

        if (this.SphereCast3D(startPosition, 0.5f, endPosition, out ShapecastHit3D hit, GroundLayer, false))
        {
            GD.Print("SLope Hit: " + hit.overlapInfo.normal.ToString());
            
            SlopeNormal = hit.overlapInfo.normal;

            if (!SlopeNormal.ApproximatelyEqual(Vector3.Up))
            {
                SlopeAngle = VectorExtensions.Angle(SlopeNormal, Vector3.Up);
                SlopeCheckHasHit = true;
                return;
            }

            SlopeAngle = 0;
            return;
        }
        SlopeAngle = 0;
        SlopeNormal = Vector3.Zero;
        SlopeCheckHasHit = false;
    }
}
