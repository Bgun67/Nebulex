using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play_Area : MonoBehaviour
{
    public List<Rigidbody> rbs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Rigidbody _rb in rbs){
            _rb.AddForce(-(_rb.transform.position - this.transform.position) *  _rb.mass * 0.1f * 60f * Time.deltaTime);
        }
    }
    void OnTriggerExit(Collider other){
        Rigidbody _otherRb = other.transform.root.GetComponent<Rigidbody>();
        if(!rbs.Contains(_otherRb)){
            rbs.Add(_otherRb);
        }
    }
     void OnTriggerEnter(Collider other){
        Rigidbody _otherRb = other.transform.root.GetComponent<Rigidbody>();
        if(rbs.Contains(_otherRb)){
            rbs.Remove(_otherRb);
        }
    }
     void OnTriggerStay(Collider other){
        Rigidbody _otherRb = other.transform.root.GetComponent<Rigidbody>();
        if(rbs.Contains(_otherRb)){
            rbs.Remove(_otherRb);
        }
    }
   
    
}
