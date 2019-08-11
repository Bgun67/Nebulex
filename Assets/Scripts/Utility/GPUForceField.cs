using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GPUForceField : MonoBehaviour
{
    public ParticleSystemRenderer particles;
    void LateUpdate(){
        Material mat= particles.sharedMaterial;
        mat.SetFloat("_Radius", transform.lossyScale.x);
        mat.SetVector("_Center", transform.position);
    }
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x);
    }

}
