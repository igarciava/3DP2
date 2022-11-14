using System.Collections.Generic;
using UnityEngine;

public class DecallPortal : MonoBehaviour
{
    public FPSPlayerController Player;
    public List<Transform> ValidPoints;
    public float MinValidDistance;
    public float MaxValidDistance;
    public float MinDotValidAngle = 0.995f;

    public bool IsValidPos(Vector3 StartPos, Vector3 Forward, float MaxDist, LayerMask PortalLayerMask, out Vector3 Position, out Vector3 Normal)
    {
        Ray l_Ray = new Ray(StartPos, Forward);
        RaycastHit l_RayCastHit;
        bool IsValid = false;
        Position = Vector3.zero;
        Normal = Vector3.forward;

        if(Physics.Raycast(l_Ray, out l_RayCastHit, MaxDist, PortalLayerMask.value))
        {
            if(l_RayCastHit.collider.tag == "PortalWall")
            {
                IsValid = true;
                Position = l_RayCastHit.point;
                Normal = l_RayCastHit.normal;
                transform.position = Position;
                transform.rotation = Quaternion.LookRotation(Normal);

                for (int i = 0; i < ValidPoints.Count; ++i)
                {
                    Vector3 l_Direction = ValidPoints[i].position - StartPos;
                    l_Direction.Normalize();
                    l_Ray = new Ray(StartPos, l_Direction);
                    if (Physics.Raycast(l_Ray, out l_RayCastHit, MaxDist))
                    {
                        if (l_RayCastHit.collider.tag == "PortalWall")
                        {
                            float l_Distance = Vector3.Distance(Position, l_RayCastHit.point);
                            Debug.Log("dist" + l_Distance);
                            float l_DotAngle = Vector3.Dot(Normal, l_RayCastHit.normal);
                            if (!(l_Distance >= MinValidDistance && l_Distance <= MaxValidDistance && l_DotAngle > MinDotValidAngle))
                            {
                                IsValid = false;
                            }
                        }
                        else
                            IsValid = false;
                    }
                    else
                        IsValid = false;
                }
            }
        }
        return IsValid;
    }
}
