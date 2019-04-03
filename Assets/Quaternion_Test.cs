using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quaternion_Test : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public float w; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = new Quaternion(x,y,z,w);
    }
}
