using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera Camera;
    public Transform OtherPortalTransform;
    public Portal MirrorPortal;
    public FPSPlayerController Player;
    public float OffsetNearPlane;
    public List<Transform> ValidPoints;
    public float MinValidDistance;
    public float MaxValidDistance;
    public float MinDotValidAngle = 0.995f;

    private void LateUpdate()
    {
        Vector3 l_WorldPos = Player.m_Camera.transform.position;
        Vector3 l_LocalPos = OtherPortalTransform.InverseTransformPoint(l_WorldPos);
        MirrorPortal.Camera.transform.position = MirrorPortal.transform.TransformPoint(l_LocalPos);

        Vector3 l_WorldDirection = Player.m_Camera.transform.forward;
        Vector3 l_LocalDirection = OtherPortalTransform.InverseTransformDirection(l_WorldDirection);
        MirrorPortal.Camera.transform.forward = MirrorPortal.transform.TransformDirection(l_LocalDirection);

        float l_Distance = Vector3.Distance(MirrorPortal.Camera.transform.position, MirrorPortal.transform.position);
        MirrorPortal.Camera.nearClipPlane = l_Distance + OffsetNearPlane;

        
    }

    public bool IsValidPos(Vector3 StartPos, Vector3 Forward, float MaxDist, LayerMask PortalLayerMask, out Vector3 Position, out Vector3 Normal)
    {
        Ray l_Ray = new Ray(StartPos, Forward);
        RaycastHit l_RayCastHit;
        bool IsValid = false;
        Position = Vector3.zero;
        Normal = Vector3.forward;

        float l_MinValidDistance = MinValidDistance * transform.localScale.x;
        float l_MaxValidDistance = MaxValidDistance * transform.localScale.y;
        Debug.Log(l_MinValidDistance+"iiiii");
        Debug.Log(l_MaxValidDistance);

        if (Physics.Raycast(l_Ray, out l_RayCastHit, MaxDist, PortalLayerMask.value))
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
                    if (Physics.Raycast(l_Ray, out l_RayCastHit, MaxDist, PortalLayerMask))
                    {
                        if (l_RayCastHit.collider.tag == "PortalWall")
                        {
                            float l_Distance = Vector3.Distance(Position, l_RayCastHit.point);
                            Debug.Log("dist" + l_Distance);
                            float l_DotAngle = Vector3.Dot(Normal, l_RayCastHit.normal);
                            if (!(l_Distance >= l_MinValidDistance && l_Distance <= l_MaxValidDistance && l_DotAngle > MinDotValidAngle))
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
