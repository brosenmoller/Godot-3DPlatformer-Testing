using System.Collections.Generic;
using PlayerStates;
using System.Linq;
using Godot;

public partial class PlayerController
{
    //public void FindGrapplePoint()
    //{
    //    if (!activePlayerActions.Contains(PlayerAction.GrappleHook))
    //    {
    //        return;
    //    }

    //    int numHitColliders = Physics.OverlapSphereNonAlloc(topPivot.position, grappleMaxDistance, grapplePointColliders, grappleable, QueryTriggerInteraction.Collide);

    //    List<RaycastHit> hits = new();
    //    for (int i = 0; i < numHitColliders; i++)
    //    {
    //        Collider collider = grapplePointColliders[i];

    //        if (collider.transform == lastGrappleObject) { continue; }

    //        Vector3 direciton = (collider.GlobalPosition - topPivot.position).Normalized();

    //        Debug.DrawLine(topPivot.GlobalPosition, topPivot.GlobalPosition + direciton * grappleMaxDistance, Color.green);

    //        if (this.RayCast3D(topPivot.position, direciton, out RaycastHit hit, grappleMaxDistance, grappleable, QueryTriggerInteraction.Collide))
    //        {
    //            if (hit.collider != collider) { continue; }
    //            if (!Camera.main.PointIsInFrustum(hit.GlobalPosition, new Vector3(0.5f, 0.5f, 0.5f))) { continue; }

    //            Vector2 hitPosXZ = new(hit.GlobalPosition.X, hit.GlobalPosition.Z);
    //            Vector2 cameraPosXZ = new(Camera.main.GlobalPosition.X, Camera.main.GlobalPosition.Z);
    //            float sqrDistanceXZCamera = Vector2.SqrMagnitude(cameraPosXZ - hitPosXZ);

    //            if (sqrDistanceXZCamera < Mathf.Pow(minDistanceCamera, 2)) { continue; }

    //            Debug.DrawLine(topPivot.position, collider.GlobalPosition, Color.magenta);

    //            hits.Add(hit);
    //        }
    //    }

    //    hits = hits.OrderBy(hit => {
    //        Vector3 viewPortPoint = Camera.main.WorldToViewportPoint(hit.GlobalPosition);
    //        viewPortPoint -= new Vector3(0.5f, 0.5f, 0);
    //        viewPortPoint.Z = 0;

    //        return viewPortPoint.LengthSquared();
    //    }).ThenBy(hit =>
    //    {
    //        return (hit.GlobalPosition - GlobalPosition).LengthSquared();
    //    }).ToList();

    //    if (hits.Count == 0)
    //    {
    //        DisableGrapplePoint();
    //        return;
    //    }

    //    SetGrapplePoint(hits[0].transform);
    //}

    //public void DisableGrapplePoint()
    //{
    //    if (selectionGrapplePoint != null) { selectionGrapplePoint = null; }
    //    if (selectedPointVisulizer.gameObject.activeInHierarchy) { selectedPointVisulizer.gameObject.SetActive(false); }
    //}

    //private void SetGrapplePoint(Transform grapplePoint)
    //{
    //    selectionGrapplePoint = grapplePoint;
    //    if (!selectedPointVisulizer.gameObject.activeInHierarchy) { selectedPointVisulizer.gameObject.SetActive(true); }
    //    selectedPointVisulizer.position = selectionGrapplePoint.position;
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //Replace me
    //    if (stateMachine.currentState.GetType() == typeof(GrapplePull) || stateMachine.currentState.GetType() == typeof(GrappleSwing)) 
    //    {
    //        stateMachine.ChangeState(typeof(AirMovement));
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.Yellow;
    //    Gizmos.DrawWireSphere(topPivot.GlobalPosition, grappleMaxDistance);
    //}
}

