using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinetic_Energy_Weapon : MonoBehaviour
{
    Rigidbody rb;
    MeshCollider collider;
    public Transform entryPoint;
    // Start is called before the first frame update
    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        collider = this.GetComponent<MeshCollider>();
    }

    void Update(){
        //rb.velocity = Vector3.ClampMagnitude(rb.velocity, 30f);
    }

    public void Activate()
    {
        rb.isKinematic = false;
        collider.enabled = false;
        StartCoroutine(CoLaunch());
    }
    
    IEnumerator CoLaunch(){
        rb.AddRelativeForce(Vector3.forward*-2000f);
        yield return new WaitForSeconds(10f);
        this.transform.position = entryPoint.position;
        this.transform.rotation = entryPoint.rotation;
        rb.useGravity = true;
        rb.velocity = Vector3.up * -30f;
        collider.enabled = true;
    }
}
