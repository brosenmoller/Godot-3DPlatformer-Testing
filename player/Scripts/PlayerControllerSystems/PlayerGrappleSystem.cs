using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PlayerController
{
    public void FindGrapplePoint()
    {
        if (!activePlayerActions.Contains(PlayerAction.GrappleHook))
        {
            return;
        }

        this.OverlapSphere3D(topPivot.GlobalPosition, grappleMaxDistance, out OverlapShape3D info, grappleable, true);

        List<RaycastHit3D> hits = new();

        for (int i = 0; i < info.allColliders.Length; i++)
        {
            CollisionObject3D collider = grapplePointColliders[i];

            if (collider == lastGrappleObject) { continue; }

            Vector3 direciton = (collider.GlobalPosition - topPivot.GlobalPosition).Normalized();

            if (this.RayCast3D(topPivot.GlobalPosition, direciton, out RaycastHit3D hit, grappleMaxDistance, grappleable, true))
            {
                if (hit.colliderInfo.collider != collider) { continue; }

                if (!mainCamera.PointIsInFrustum(hit.colliderInfo.collider.GlobalPosition, new Vector3(0.5f, 0.5f, 0.5f))) { continue; }

                Vector2 hitPosXZ = new(hit.colliderInfo.collider.GlobalPosition.X, hit.colliderInfo.collider.GlobalPosition.Z);
                Vector2 cameraPosXZ = new(mainCamera.GlobalPosition.X, mainCamera.GlobalPosition.Z);
                float sqrDistanceXZCamera = (cameraPosXZ - hitPosXZ).LengthSquared();

                if (sqrDistanceXZCamera < Mathf.Pow(minDistanceCamera, 2)) { continue; }

                hits.Add(hit);
            }
        }

        hits = hits.OrderBy(hit =>
        {
            Vector2 viewPortPoint = mainCamera.UnprojectPosition(hit.colliderInfo.collider.GlobalPosition);
            viewPortPoint -= new Vector2(0.5f, 0.5f);

            return viewPortPoint.LengthSquared();
        }).ThenBy(hit =>
        {
            return (hit.colliderInfo.collider.GlobalPosition - GlobalPosition).LengthSquared();
        }).ToList();

        if (hits.Count == 0)
        {
            DisableGrapplePoint();
            return;
        }

        SetGrapplePoint(hits[0].colliderInfo.collider);
    }

    public void DisableGrapplePoint()
    {
        if (selectionGrapplePoint != null) { selectionGrapplePoint = null; }

        if (selectedPointVisulizer.IsProcessing()) 
        {
            selectedPointVisulizer.Visible = false;
            selectedPointVisulizer.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private void SetGrapplePoint(Node3D grapplePoint)
    {
        selectionGrapplePoint = grapplePoint;
        if (!selectedPointVisulizer.IsProcessing()) 
        {
            selectedPointVisulizer.Visible = true;
            selectedPointVisulizer.ProcessMode = ProcessModeEnum.Inherit;
        }

        selectedPointVisulizer.GlobalPosition = selectionGrapplePoint.GlobalPosition;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //Replace me
    //    if (stateMachine.currentState.GetType() == typeof(GrapplePull) || stateMachine.currentState.GetType() == typeof(GrappleSwing))
    //    {
    //        stateMachine.ChangeState(typeof(AirMovement));
    //    }
    //}
}

