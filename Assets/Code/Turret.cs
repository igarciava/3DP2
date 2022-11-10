using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Turret : MonoBehaviour
{
    public LineRenderer Laser;
    public LayerMask LaserLayerMask;
    public float MaxLaserDist = 250.0f;
    public float AlifeAngleInDegrees = 30.0f;

    void Update()
    {
        bool l_LaserAlife = Vector3.Dot(transform.up, Vector3.up) > Mathf.Cos(AlifeAngleInDegrees * Mathf.Deg2Rad);
        Laser.gameObject.SetActive(l_LaserAlife);
        if(l_LaserAlife)
        {
            Ray l_Ray = new Ray(Laser.transform.position, Laser.transform.forward);
            float l_LaserDistance = MaxLaserDist;
            RaycastHit l_RayCastHit;
            if (Physics.Raycast(l_Ray, out l_RayCastHit, MaxLaserDist, LaserLayerMask.value))
            {
                l_LaserDistance = Vector3.Distance(Laser.transform.position, l_RayCastHit.point);
                if (l_RayCastHit.collider.tag == "RefractionCube")
                {
                    l_RayCastHit.collider.GetComponent<RefractionCube>().CreateRefraction();
                }
                if (l_RayCastHit.collider.tag == "Player")
                {
                    l_RayCastHit.collider.GetComponent<FPSPlayerController>().Die();
                }
            }
            Laser.SetPosition(1, new Vector3(0.0f, 0.0f, l_LaserDistance));
        }
    }
}
