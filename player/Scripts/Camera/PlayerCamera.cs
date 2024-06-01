using Godot;
using MEC;
using System.Collections.Generic;

public partial class PlayerCamera : Node3D
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

    private InputService input;
    private Vector2 cameraInput;


    [ExportCategory("Controls")]
    [Export]
    private Camera3D cameraLook;

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

    [Export]
    public float collisionRadius;

    [Export(PropertyHint.Layers3DPhysics)]
    public uint groundCollisionMask;

    [Export(PropertyHint.Layers3DPhysics)]
    private uint playerAndGroundCollisionMask;

    [Export]
    private string collisionIgnoreTag;

    [Export]
    private float returnToNormalAfterCollisionSpeed;

    [Export]
    private float whiskerOriginRecheckHeight;

    private CollisionObject3D[] colliderBuffer;
    private Vector3 desiredCameraLookPosition;
    private PlayerController player;
    private Vector3 smoothVelocity;

    private Vector3[] verticalWhiskers;

    private void Awake()
    {
        verticalWhiskers = new Vector3[8];

        currentCameraLookOffset = cameraLookOffset;

        player = this.GetNodeByType<PlayerController>();

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void Start()
    {
        input = ServiceLocator.Instance.Get<InputService>();

        input.Bindings["Camera"].Performed += CameraInput;
        input.Bindings["Camera"].Canceled += CancelCamera;

        input.Bindings["recenter"].Performed += RecenterCamera;
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Bindings["Camera"].Performed -= CameraInput;
            input.Bindings["Camera"].Canceled -= CancelCamera;

            input.Bindings["recenter"].Performed -= RecenterCamera;
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

    private void CameraInput()
    {
        cameraInput = input.Bindings["Camera"].ReadValue<Vector2>();
    }

    private void CancelCamera()
    {
        cameraInput = Vector2.Zero;
    }

    private void RotateCameraLook()
    {
        Vector3 cameraDirection = (GlobalPosition - cameraLook.GlobalPosition).Normalized();

        float step = cameraLookSpeed * this.PhysicsDelta();

        this.SetForward(cameraLook.Transform.Forward().Lerp(cameraDirection, step));
    }

    private void RotateBase()
    {
        if (cameraInput.LengthSquared() < 0.1f)
        {
            return;
        }

        CancelRecenterCamera();

        Vector2 rotation = GlobalTransform.Basis.GetEuler().XY();

        if (rotation.X > 180)
        {
            rotation.X -= 360;
        }

        float xMultiplier = invertX ? -1 : 1;
        float yMultiplier = invertY ? -1 : 1;

        float inputDevice = 1;
        if (inputDevice == 1)
        {
            rotation.Y += cameraInput.X * inputSensitivityMouse.X * this.PhysicsDelta() * xMultiplier;
            rotation.X += cameraInput.Y * inputSensitivityMouse.Y * this.PhysicsDelta() * yMultiplier;
        }
        else
        {
            rotation.Y += cameraInput.X * inputSensitivityGamePad.X * this.PhysicsDelta() * xMultiplier;
            rotation.X += cameraInput.Y * inputSensitivityGamePad.Y * this.PhysicsDelta() * yMultiplier;
        }

        rotation.X = Mathf.Clamp(rotation.X, clampAngleMin, clampAngleMax);

        Quaternion orbitQuaternion = Quaternion.FromEuler(new Vector3(rotation.X, rotation.Y, 0.0f));
        Basis = new Basis(GlobalTransform.Basis.GetRotationQuaternion().Slerp(orbitQuaternion, this.PhysicsDelta() * 10));
    }

    private Vector3 CalculateDesiredCameraLookPosition()
    {
        return GlobalPosition + cameraLookOffset;
    }

    private void MoveBase()
    {
        desiredCameraLookPosition = CalculateDesiredCameraLookPosition();

        ShapecastHit3D hit;

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
            desiredCameraLookPosition = GlobalPosition + currentCameraLookOffset;

            // Move Gradually backwards
            if (Mathf.Abs(cameraLookOffset.Z) > Mathf.Abs(currentCameraLookOffset.Z))
            {
                float step = returnToNormalAfterCollisionSpeed * this.PhysicsDelta();
                currentCameraLookOffset = currentCameraLookOffset.SmoothDamp(cameraLookOffset, ref smoothVelocity, step, this.PhysicsDelta());
            }
        }

        cameraLook.Position = desiredCameraLookPosition;
    }

    private bool IsLineOffSightBroken(out ShapecastHit3D hit)
    {
        // Vertical Whiskers to detect if there is a point above or slightly below the player not obstructed by collisions (this allows player hiding behind small ledges)
        float downDistance = 2.0f;
        float upDistance = maxDistanceFromPlayer;

        if (this.RayCast3D(GlobalPosition, Vector3.Down, out RaycastHit3D downHit, downDistance, groundCollisionMask))
        {
            downDistance = downHit.distance;
        }
        if (this.RayCast3D(GlobalPosition, Vector3.Up, out RaycastHit3D upHit, upDistance, groundCollisionMask))
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
        if (this.RayCast3D(desiredCameraLookPosition, Vector3.Up, out upHit, whiskerOriginRecheckHeight, groundCollisionMask))
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

                // Should Ignore Tag "collisionIgnoreTag"
                if (!this.RayCast3D(checkPosition, direciton, direciton.Length(), groundCollisionMask))
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
            hit = new ShapecastHit3D();
            return false;
        }

        // Do normal sphere cast check
        Vector3 direction = GlobalPosition.DirectionTo(desiredCameraLookPosition);
        Vector3 endPosition = GlobalPosition + direction * maxDistanceFromPlayer;

        // Should Ignore Tag "collisionIgnoreTag"
        return this.SphereCast3D(GlobalPosition, collisionRadius, endPosition, out hit, groundCollisionMask);
    }

    private bool IsCameraInsideCollider(out ShapecastHit3D hit)
    {
        Vector3 direction = (desiredCameraLookPosition - GlobalPosition).Normalized();
        Vector3 frontHitPoint = GlobalPosition;

        if (this.RayCast3D(desiredCameraLookPosition, -direction, out RaycastHit3D frontHit, maxDistanceFromPlayer, groundCollisionMask))
        {
            frontHitPoint = frontHit.point;
        }

        float distance = (frontHitPoint - desiredCameraLookPosition).Length();

        if (this.RayCast3D(frontHitPoint, direction, out RaycastHit3D backHit, distance, groundCollisionMask))
        {
            //string ignoreTag = backHit.colliderInfo.collider.IsInGroup(collisionIgnoreTag) ? "" : collisionIgnoreTag;

            if (backHit.distance < collisionRadius * 2)
            {
                //if (PhysicsExtension.SphereCastIgnoreTag(GlobalPosition, collisionRadius, direction, out hit, distance, groundCollisionMask, ignoreTag))
                //{
                //    return true;
                //}

                // Should Ignore Tag "collisionIgnoreTag"
                if (this.SphereCast3D(GlobalPosition, collisionRadius, direction, out hit, distance, groundCollisionMask))
                {
                    return true;
                }
            }

            // Should Ignore Tag "collisionIgnoreTag"
            if (this.SphereCast3D(backHit.point - direction * (collisionRadius * 2.0f), collisionRadius, direction, out hit, collisionRadius * 3.0f, groundCollisionMask))
            {
                return true;
            }

            // Should Ignore Tag "collisionIgnoreTag"
            if (this.SphereCast3D(frontHitPoint, collisionRadius, direction, out hit, distance, groundCollisionMask))
            {
                return true;
            }

            //if (PhysicsExtension.SphereCastIgnoreTag(backHit.point - direction * (collisionRadius * 2.0f), collisionRadius, direction, out hit, collisionRadius * 3.0f, groundCollisionMask, ignoreTag))
            //{
            //    return true;
            //}
            //else if (PhysicsExtension.SphereCastIgnoreTag(frontHitPoint, collisionRadius, direction, out hit, distance, groundCollisionMask, ignoreTag))
            //{
            //    return true;
            //}
        }

        hit = new ShapecastHit3D();
        return false;
    }

    private void HandleCameraCollision(in ShapecastHit3D hit)
    {
        // Check if colission is vertical, horizontal or diagonal
        float absoluteDotProduct = Mathf.Abs(hit.overlapInfo.normal.Dot(Vector3.Up));

        CollisionType collisionType = absoluteDotProduct < 0.1f ? CollisionType.Horizontal : 
                                      absoluteDotProduct < 0.9f ? CollisionType.Diagonal : 
                                      CollisionType.Vertical;

        Vector3 newCameraPosition = hit.lastSafeLocation;

        if (preserveCameraHeight)
        {
            if (collisionType == CollisionType.Horizontal)
            {
                Vector3 desiredPreserveHeightPosition = newCameraPosition;
                desiredPreserveHeightPosition.Y = desiredCameraLookPosition.Y;

                Vector3 newCameraXZBaseY = new(newCameraPosition.X, GlobalPosition.Y, newCameraPosition.Z);
                float distance = Mathf.Abs(newCameraXZBaseY.Y - desiredPreserveHeightPosition.Y);

                Vector3 rayDirection = desiredPreserveHeightPosition.Y < newCameraPosition.Y ? Vector3.Down : Vector3.Up;

                if (this.RayCast3D(newCameraXZBaseY, rayDirection, out RaycastHit3D hitInfo, maxDistanceFromPlayer + collisionRadius, groundCollisionMask))
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

        currentCameraLookOffset = newCameraPosition - GlobalPosition;
        currentCameraLookOffset.Y = cameraLookOffset.Y;
        currentCameraLookOffset.X = cameraLookOffset.X;

        desiredCameraLookPosition = newCameraPosition;
    }


    private void RecenterCamera()
    {
        if (useLerp) { recenterRoutine = Timing.RunCoroutine(RecenteringCameraLerp(), Segment.Process); }
        else { recenterRoutine = Timing.RunCoroutine(RecenteringCamera(), Segment.Process); }
    }

    private void CancelRecenterCamera()
    {
        Timing.KillCoroutines(recenterRoutine);
    }

    private IEnumerator<double> RecenteringCameraLerp()
    {
        Vector3 startForward = Transform.Forward();
        Vector3 endForward = player.Transform.Forward();

        float angle = VectorExtensions.Angle(Transform.Forward(), player.Transform.Forward());
        float duration = angle / recenteringSpeed;

        float t = 0f;
        while (t < 1f)
        {
            t += this.PhysicsDelta() / duration;

            this.SetForward(startForward.Lerp(endForward, Mathf.Pow(t, 1.5f)));

            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<double> RecenteringCamera()
    {
        while (!Basis.GetEuler().ApproximatelyEqual(player.Basis.GetEuler()))
        {
            float yStep = recenteringSpeedX * this.PhysicsDelta();
            float yRotation = Mathf.MoveToward(GlobalBasis.GetEuler().Y, player.GlobalBasis.GetEuler().Y, yStep);

            float xStep = recenteringSpeedY * this.PhysicsDelta();
            float xRotation = Mathf.MoveToward(GlobalBasis.GetEuler().X, player.GlobalBasis.GetEuler().X, xStep);

            Basis = new Basis(Quaternion.FromEuler(new Vector3(xRotation, yRotation, 0.0f)));

            yield return Timing.WaitForOneFrame;
        }
    }
}
