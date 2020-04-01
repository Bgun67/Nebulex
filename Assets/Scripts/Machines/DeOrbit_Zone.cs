using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeOrbit_Zone : MonoBehaviour
{
    public Transform reentryPoint; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter (Collider other)
    {
        Reentry_Vehicle vehicle = other.GetComponent<Reentry_Vehicle>();
        if(vehicle != null){
            vehicle.DeOrbit(reentryPoint.position);
        }
    }
}
