using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterOceanMask : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       GetComponent<Renderer>().sortingOrder = -3000 - 1; 
    }

}
