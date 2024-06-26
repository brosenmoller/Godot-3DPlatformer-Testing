﻿using Godot;
using MEC;

namespace PlayerStates
{
    public class WallSlideDown : State<PlayerController>
    {
        public override void OnEnter()
        {
            ctx.InvokeOnWallDownEnter();

            ctx.UseGravity = false;
            ctx.climbRotateCoroutine = Timing.RunCoroutine(ctx.RotateTo(-ctx.WallNormal), Segment.PhysicsProcess);
        }

        public override void OnPhysicsUpdate()
        {
            ctx.Velocity = Vector3.Up * PlayerController.GRAVITY / 3;
            //Is all of this needed? IDK it works though
            Vector3 cross = ctx.WallNormal.Cross(Vector3.Up);
            //determine if the projected input is going in a left or right direction
            float leftRightInput = ctx.InputDirection.Dot(cross);
            //determine if the projected input is going in a forward or backwards direction
            float forwardBackInput = ctx.InputDirection.Dot(ctx.Transform.Forward());

            if (leftRightInput < -0.4f)
            {
                ctx.GlobalPosition += 4 * ctx.PhysicsDelta() * -cross;
                return;
            }
            if (leftRightInput > 0.4f)
            {
                ctx.GlobalPosition += 4 * ctx.PhysicsDelta() * cross;
                return;
            }
            if (forwardBackInput < -0.4f)
            {
                ctx.wallMoveBackTime += ctx.PhysicsDelta();
            }
            else
            {
                ctx.wallMoveBackTime = 0;
            }
        }

        public override void OnExit()
        {
            ctx.InvokeOnWallDownExit();

            ctx.UseGravity = true;
            Timing.KillCoroutines(ctx.climbRotateCoroutine);
        }
    }
}
