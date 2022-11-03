using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent Action;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
            Action.Invoke();
        }
    }
}
