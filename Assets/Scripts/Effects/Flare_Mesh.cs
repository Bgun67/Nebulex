using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flare_Mesh : MonoBehaviour
{
    [SerializeField]
    LensFlare flare;
    public LayerMask mask;

    void Update(){
        flare.enabled = false;
        if(this.GetComponent<Renderer>().isVisible){
            Bounds _bounds = this.GetComponent<Renderer>().bounds;
            if(!Physics.Raycast(Camera.current.transform.position,_bounds.center + _bounds.max - Camera.current.transform.position, mask)){
                flare.enabled = true;
            }
        }
    }
    void OnBecameInvisible(){
       
    }
}
