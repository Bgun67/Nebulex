using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowChain : MonoBehaviour
{
    public float linkDistance;
    public Rigidbody end1;
    public Rigidbody end2;
    const int numLinks = 5;
    const float youngsModulus = 1E6f;
    private Vector3[] linkPositions = new Vector3[numLinks];
    private Vector3[] linkVelocities = new Vector3[numLinks];
    private Vector3[] linkForces = new Vector3[numLinks];
    LineRenderer chainRenderer;

    void Start(){
        chainRenderer = GetComponent<LineRenderer>();
        for(int i = 0; i<numLinks; i++){
            linkPositions[i] = Vector3.Lerp(end1.position, end2.position, (float)i/(float)numLinks);
        }
        chainRenderer.SetPositions(linkPositions);
    }
    void FixedUpdate(){
        linkPositions[0] = end1.position;
        linkPositions[numLinks-1] = end2.position;

        linkVelocities[0] = end1.velocity;
        linkVelocities[numLinks-1] = end2.velocity;

        if(Vector3.Distance(end1.position, end2.position) > numLinks * linkDistance){
            //Chain tight, redistribute links
            for(int i = 0; i<numLinks; i++){
                linkPositions[i] = Vector3.Lerp(end1.position, end2.position, (float)i/(float)numLinks);
            }
        }

        Vector3 _displacement = linkPositions[0] - linkPositions[1];
        linkForces[0] = _displacement.normalized*Mathf.Min(Mathf.Max(0f, _displacement.magnitude - linkDistance),0.01f) * -youngsModulus;
        for(int i =1; i<numLinks-1; i++){
            _displacement = linkPositions[i] - linkPositions[i-1];
            linkForces[i] = _displacement.normalized*Mathf.Min(Mathf.Max(0f, _displacement.magnitude - linkDistance),0.01f) * -youngsModulus;
            _displacement = linkPositions[i] - linkPositions[i+1];
            linkForces[i] += _displacement.normalized*Mathf.Min(Mathf.Max(0f, _displacement.magnitude - linkDistance),0.01f) * -youngsModulus;
        }
        _displacement = linkPositions[numLinks-1] - linkPositions[numLinks-2];
        linkForces[numLinks-1] += _displacement.normalized*Mathf.Min(Mathf.Max(0f, _displacement.magnitude - linkDistance),0.01f) * -youngsModulus;

        for(int i = 1; i<numLinks-1; i++){
            linkVelocities[i] += linkForces[i] / 4f * Time.fixedDeltaTime;
            //linkVelocities[i] = linkVelocities[i] * 0.5f;
            linkPositions[i] += linkVelocities[i]*Time.fixedDeltaTime;
        }
        chainRenderer.SetPositions(linkPositions);


        //Display
        for(int i = 0; i<numLinks-1; i++){
            Debug.DrawLine(linkPositions[i], linkPositions[i+1]);
        }

        _displacement = end2.position - end1.position;
        Vector3 _bodyForce = _displacement.normalized*Mathf.Min(Mathf.Max(0f, _displacement.magnitude - linkDistance * numLinks),0.01f) * -youngsModulus; 
        end1.AddForce(-_bodyForce);
        end2.AddForce(_bodyForce);
        
        
    }
}
