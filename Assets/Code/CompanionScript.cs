using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionScript : MonoBehaviour
{
    bool IsAttached = false;
    public Rigidbody Rigidbody;
    float OffsetTeleportPortal = 1.5f;
    Portal ExitPortal = null;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void SetAttached(bool Attached)
    {
        IsAttached = Attached;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Portal" && !IsAttached)
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if(l_Portal != ExitPortal)
            {
                Teleport(l_Portal);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Portal")
        {
            if(other.GetComponent<Portal>()==ExitPortal)
            {
                ExitPortal = null;
            }
        }
    }

    void Teleport(Portal thePortal)
    {
        Vector3 l_LocalPos = thePortal.OtherPortalTransform.InverseTransformPoint(transform.position);
        Vector3 l_LocalDir = thePortal.OtherPortalTransform.transform.InverseTransformDirection(transform.forward);

        Vector3 l_LocalVelocity = thePortal.OtherPortalTransform.transform.InverseTransformDirection(Rigidbody.velocity);

        Vector3 l_WorldVelocity = thePortal.MirrorPortal.transform.TransformDirection(l_LocalVelocity);

        Rigidbody.isKinematic = true;
        transform.forward = thePortal.MirrorPortal.transform.TransformDirection(l_LocalDir);
        Vector3 l_WorldVelocityNormalized = l_WorldVelocity.normalized;
        transform.position = thePortal.MirrorPortal.transform.TransformPoint(l_LocalPos) + l_WorldVelocityNormalized * OffsetTeleportPortal;
        transform.localScale *= (thePortal.MirrorPortal.transform.localScale.x / thePortal.transform.localScale.x);
        Rigidbody.isKinematic = false;
        Rigidbody.velocity = l_WorldVelocity;
        ExitPortal = thePortal.MirrorPortal;
    }
}
